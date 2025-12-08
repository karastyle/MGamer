using UnityEngine;
using UnityEngine.Rendering;

// TreeManager_Optimized.cs
// 适配Segmented Big Buffer架构
public class TreeManager : MonoBehaviour
{
    [Header("Data")] public TextAsset treeInstancesJson;
    public GameObject[] prototypePrefabs;

    [HideInInspector] public TreeInstancesData treeData;

    [Header("Shaders")] public Shader treeInstancedShader;
    public ComputeShader megaCullShader;
    public ComputeShader hzbGeneratorShader;
    public bool useHZB;
    
    [Header("Debug")]
    public bool bDebug = false;
    public float debugReadbackInterval = 1.0f;
    
    private TreeInstanceCore instanceCore;
    private CommandBuffer renderCommandBuffer;

    // 调试统计信息
    [HideInInspector] public uint totalInstances;
    [HideInInspector] public uint afterFrustumCulling;
    [HideInInspector] public uint afterLODCulling;
    [HideInInspector] public uint afterHZBCulling;
    [HideInInspector] public uint finalTris;
    [HideInInspector] public uint finalVerts;
    
    private float debugReadbackTimer = 0f;

    
    // ✅ 从JSON加载数据
    public void LoadTreeData()
    {
        if (treeInstancesJson == null)
        {
            Debug.LogWarning("[TreeManager] Tree Instances JSON is not assigned!");
            treeData = null;
            return;
        }

        try
        {
            treeData = JsonUtility.FromJson<TreeInstancesData>(treeInstancesJson.text);

            if (treeData == null)
            {
                Debug.LogError("[TreeManager] Failed to parse JSON data!");
                return;
            }

            // 初始化prototypePrefabs数组（如果需要）
            if (treeData.prototypes != null &&
                (prototypePrefabs == null || prototypePrefabs.Length != treeData.prototypes.Count))
            {
                prototypePrefabs = new GameObject[treeData.prototypes.Count];
            }

            Debug.Log(
                $"[TreeManager] Loaded tree data: {treeData.instances?.Count ?? 0} instances, {treeData.prototypes?.Count ?? 0} prototypes");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TreeManager] Error loading tree data: {e.Message}");
            treeData = null;
        }
    }

    // ✅ 初始化GPU实例化
    public void InitializeGPUInstancing()
    {
        if (treeData == null)
        {
            LoadTreeData();
        }

        if (treeData == null || prototypePrefabs == null || prototypePrefabs.Length == 0)
        {
            Debug.LogError("[TreeManager] Missing tree data or prefabs!");
            return;
        }

        // 清理旧资源
        CleanupGPUInstancing();

        // ✅ 使用新的初始化方法
        instanceCore = new TreeInstanceCore();
        instanceCore.Initialize(
            treeData,
            prototypePrefabs,
            treeInstancedShader,
            megaCullShader,
            hzbGeneratorShader
        );

        renderCommandBuffer = new CommandBuffer { name = "TreeRendering_Optimized" };

        Debug.Log("[TreeManager] GPU Instancing initialized with Segmented Big Buffer");
    }

    // ✅ 清理GPU实例化
    public void CleanupGPUInstancing()
    {
        instanceCore?.Dispose();
        instanceCore = null;

        renderCommandBuffer?.Dispose();
        renderCommandBuffer = null;
    }

    void Start()
    {
        LoadTreeData();

        InitializeGPUInstancing();
    }

    void Update()
    {
        if (instanceCore == null || !instanceCore.IsInitialized)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        renderCommandBuffer.Clear();
        instanceCore.Render(mainCamera, renderCommandBuffer, useHZB, bDebug);
        Graphics.ExecuteCommandBuffer(renderCommandBuffer);

        // 调试回读
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

    private void ReadDebugData()
    {
        if (instanceCore != null)
        {
            instanceCore.ReadDebugCounters();
            
            totalInstances = instanceCore.debugCounters[0];
            afterFrustumCulling = instanceCore.debugCounters[1];
            afterLODCulling = instanceCore.debugCounters[2];
            afterHZBCulling = instanceCore.debugCounters[3];
            finalTris = instanceCore.debugCounters[4];
            finalVerts = instanceCore.debugCounters[5];
        }
    }

    void OnDestroy()
    {
        CleanupGPUInstancing();
    }
}