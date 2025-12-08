using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.IO;

public class DetailManager : MonoBehaviour
{
    [System.Serializable]
    public class DetailLayerData
    {
        public Texture2D densityMap;
        public TextAsset metadataJson;
        public GameObject prefab;
        [HideInInspector] public DetailMetadata metadata;
    }

    [Header("Data Source")]
    public TextAsset detailInstancesJson;
    public List<DetailLayerData> detailLayers = new List<DetailLayerData>();
    
    [Header("Terrain Settings")]
    public Vector3 terrainPosition;
    public Vector3 terrainSize = new Vector3(1000, 600, 1000);
    public Texture2D heightMap;
    
    [Header("Runtime Settings")]
    [Range(0, 2)]
    public float grassDensity = 1;
    public float maxDistance = 200;
    public float fadeStart = 100;
    public float maxDrawLayer = 1000;
    
    [Header("Culling Settings")]
    [Range(0.0f, 0.1f)]
    public float occlusionOffset = 0.002f;
    
    [Header("Rendering")]
    public bool enableFrustumCulling = true;
    public bool enableHZBCulling = false;
    
    [Header("Debug")]
    public bool bDebug = false;
    public float debugReadbackInterval = 1.0f;
    
    [Header("Shaders")]
    public Shader instancedShader;
    public ComputeShader grassGenerateShader;
    public ComputeShader frustumCullingShader;
    public ComputeShader hzbGeneratorShader;
    public ComputeShader hzbCullingShader;

    private List<DetailInstanceCore> instanceCores = new List<DetailInstanceCore>();
    private HZBOcclusionCuller globalHzbCuller;
    private CommandBuffer globalCmd;

    // 调试统计信息（所有layer累加）
    [HideInInspector] public uint totalInstances;
    [HideInInspector] public uint afterFrustumCulling;
    [HideInInspector] public uint afterHZBCulling;
    [HideInInspector] public uint finalTris;
    [HideInInspector] public uint finalVerts;
    
    private float debugReadbackTimer = 0f;
    
    private void OnEnable()
    {
        globalCmd = new CommandBuffer();
        globalCmd.name = "DetailManager_GlobalCmd";
        
        if (Application.isPlaying)
        {
            Initialize();
        }
    }

    private void OnDisable()
    {
        Cleanup();
        
        if (globalCmd != null)
        {
            globalCmd.Release();
            globalCmd = null;
        }
    }

    private void Update()
    {
        if (instanceCores != null && instanceCores.Count > 0)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                globalCmd.Clear();
                
                if (enableHZBCulling && globalHzbCuller != null)
                {
                    globalHzbCuller.GenerateHZB(globalCmd, cam);
                }

                foreach (var core in instanceCores)
                {
                    if (core != null && core.IsInitialized)
                    {
                        core.Render(globalCmd, cam, enableFrustumCulling, enableHZBCulling, occlusionOffset, globalHzbCuller, bDebug);
                    }
                }
                
                Graphics.ExecuteCommandBuffer(globalCmd);

                // 定时回读调试数据
                if (bDebug)
                {
                    debugReadbackTimer += Time.deltaTime;
                    if (debugReadbackTimer >= debugReadbackInterval)
                    {
                        debugReadbackTimer = 0f;
                        ReadDebugData();
                    }
                }
            }
        }
    }

    private void ReadDebugData()
    {
        totalInstances = 0;
        afterFrustumCulling = 0;
        afterHZBCulling = 0;
        finalTris = 0;
        finalVerts = 0;

        foreach (var core in instanceCores)
        {
            if (core != null && core.IsInitialized)
            {
                core.ReadDebugCounters();
                
                totalInstances += core.debugCounters[0];
                afterFrustumCulling += core.debugCounters[1];
                afterHZBCulling += core.debugCounters[2];
                finalTris += core.debugCounters[3];
                finalVerts += core.debugCounters[4];
            }
        }
    }

    public void Initialize()
    {
        Cleanup();

        if (grassGenerateShader == null)
        {
            Debug.LogError("DetailManager: Missing Shaders.");
            return;
        }

        if (detailLayers.Count == 0)
        {
            Debug.LogError("DetailManager: No detail layers to render.");
            return;
        }

        Texture2D activeHeightMap = heightMap;
        if (activeHeightMap == null)
        {
            activeHeightMap = Texture2D.whiteTexture;
            Debug.LogWarning("DetailManager: HeightMap not assigned, using flat height.");
        }

        if (enableHZBCulling)
        {
            globalHzbCuller = new HZBOcclusionCuller();
            if (hzbGeneratorShader != null && hzbCullingShader != null)
            {
                globalHzbCuller.Initialize(hzbGeneratorShader, hzbCullingShader);
            }
        }

        int layerIndex = 0;
        int totalInstances = 0;
        foreach (var layer in detailLayers)
        {
            if (layerIndex >= maxDrawLayer)
                break;
            
            if (layer.densityMap == null || layer.metadataJson == null || layer.prefab == null)
            {
                Debug.LogWarning("DetailManager: Skipping incomplete layer.");
                layerIndex++;
                continue;
            }

            try
            {
                layer.metadata = JsonUtility.FromJson<DetailMetadata>(layer.metadataJson.text);
            }
            catch
            {
                Debug.LogError($"DetailManager: Failed to parse metadata for layer.");
                layerIndex++;
                continue;
            }

            DetailInstanceCore core = new DetailInstanceCore();
            
            core.Initialize(
                layer.densityMap, activeHeightMap, layer.metadata,
                terrainPosition, terrainSize, grassDensity, maxDistance, fadeStart,
                layer.prefab,
                instancedShader,
                grassGenerateShader,
                frustumCullingShader,
                null, null
            );
            
            instanceCores.Add(core);
            layerIndex++;
            totalInstances += core.generatedCount;
        }
        
        Debug.Log($"DetailManager: Initialized {instanceCores.Count} detail layers.   Total instances: {totalInstances}");
    }

    public void Cleanup()
    {
        if (instanceCores != null)
        {
            foreach (var core in instanceCores)
            {
                core?.Dispose();
            }
            instanceCores.Clear();
        }
        
        if (globalHzbCuller != null)
        {
            globalHzbCuller.Dispose();
            globalHzbCuller = null;
        }
    }

    public void ParseDetailInstances(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath))
            return;

        string json;
        
        #if UNITY_EDITOR
        string fullPath = Path.Combine(Application.dataPath, jsonPath.Replace("Assets/", ""));
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"DetailInstances file not found: {fullPath}");
            return;
        }
        json = File.ReadAllText(fullPath);
        #else
        TextAsset jsonAsset = detailInstancesJson;
        if (jsonAsset == null)
        {
            Debug.LogError("DetailInstances json not assigned.");
            return;
        }
        json = jsonAsset.text;
        #endif

        TerrainExporter.DetailInstancesData data = JsonUtility.FromJson<TerrainExporter.DetailInstancesData>(json);

        if (data == null)
        {
            Debug.LogError("Failed to parse DetailInstances.json");
            return;
        }

        terrainPosition = data.terrainPosition;
        terrainSize = data.terrainSize;

        #if UNITY_EDITOR
        string directory = Path.GetDirectoryName(jsonPath);

        detailLayers.Clear();

        foreach (var proto in data.prototypes)
        {
            DetailLayerData layer = new DetailLayerData();

            string densityPath = Path.Combine(directory, proto.densityMapPath);
            string metadataPath = Path.Combine(directory, proto.metadataPath);

            layer.densityMap = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(densityPath);
            layer.metadataJson = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(metadataPath);

            if (!string.IsNullOrEmpty(proto.prefabPath))
            {
                layer.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(proto.prefabPath);
            }

            if (layer.metadataJson != null)
            {
                try
                {
                    layer.metadata = JsonUtility.FromJson<DetailMetadata>(layer.metadataJson.text);
                }
                catch
                {
                    Debug.LogWarning($"Failed to parse metadata for layer {proto.index}");
                }
            }

            detailLayers.Add(layer);
        }

        Debug.Log($"Parsed DetailInstances: {detailLayers.Count} layers loaded.");
        #endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(terrainPosition + terrainSize * 0.5f, terrainSize);
    }
}