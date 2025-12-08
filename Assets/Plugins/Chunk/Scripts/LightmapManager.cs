using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using EasyTools; 
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightmapManager : MonoBehaviour
{
    [System.Serializable]
    public class LightmapRendererInfo
    {
        public string objectPath;
        public int finalGlobalIndex;
        public Vector4 lightmapScaleOffset; 
    }

    [System.Serializable]
    public class TextureInfo
    {
        public int globalIndex;
        public string texturePath;
    }

    [System.Serializable]
    public class SceneLightmapData
    {
        public string sceneName;
        // 【注意】这里是 textures，不再是 lightmapTexturePaths
        public List<TextureInfo> textures = new List<TextureInfo>();
        public List<LightmapRendererInfo> rendererInfos = new List<LightmapRendererInfo>();
    }

    public List<SceneLightmapData> allSceneData = new List<SceneLightmapData>();
    private LightmapData[] globalLightmapArray;

    private void Awake()
    {
        InitGlobalLightmaps();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Scene baseScene = SceneManager.GetActiveScene();
        if (baseScene.isLoaded) StartCoroutine(LoadSceneLightmaps(baseScene));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnloadLightmap();
    }

    private void InitGlobalLightmaps()
    {
        int maxIndexNeeded = 0;
        foreach (var data in allSceneData)
        {
            foreach(var tex in data.textures)
            {
                if (tex.globalIndex > maxIndexNeeded) maxIndexNeeded = tex.globalIndex;
            }
        }

        int size = maxIndexNeeded + 1;
        if (size > 0)
        {
            globalLightmapArray = new LightmapData[size];
            for (int i = 0; i < size; i++) globalLightmapArray[i] = new LightmapData();
            LightmapSettings.lightmaps = globalLightmapArray;
            LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(LoadSceneLightmaps(scene));
    }

    private IEnumerator LoadSceneLightmaps(Scene scene)
    {
        SceneLightmapData data = allSceneData.Find(d => d.sceneName == scene.name);
        if (data == null) yield break;

        // 1. 填空
        foreach (var texInfo in data.textures)
        {
            int targetIndex = texInfo.globalIndex;
            if (globalLightmapArray[targetIndex].lightmapColor != null) continue;

            Texture2D tex = null;
            if (EasyAsset.Instance == null || !EasyAsset.Instance.HasInitialized())
            {
#if UNITY_EDITOR
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texInfo.texturePath);
#endif
            }
            else
            {
                var handle = EasyAsset.Instance.LoadAssetAsync(texInfo.texturePath);
                yield return handle.WaitForCompletion();
                tex = handle.AssetObject<Texture2D>();
            }

            if (tex != null) globalLightmapArray[targetIndex].lightmapColor = tex;
        }
        LightmapSettings.lightmaps = globalLightmapArray;

        // 2. 应用
        foreach (var info in data.rendererInfos)
        {
            GameObject obj = FindObjectInSpecificScene(info.objectPath);
            if (obj != null)
            {
                MeshRenderer rend = obj.GetComponent<MeshRenderer>();
                if (rend != null)
                {
                    rend.lightmapIndex = info.finalGlobalIndex;
                    rend.lightmapScaleOffset = info.lightmapScaleOffset;
                }
            }
        }
    }

    public void UnloadLightmap()
    {
        LightmapSettings.lightmaps = new LightmapData[0];
        globalLightmapArray = null;
    }

    private GameObject FindObjectInSpecificScene(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return null;
        string[] parts = fullPath.Split('/');
        if (parts.Length < 2) return null;
        Scene scene = SceneManager.GetSceneByName(parts[0]);
        if (!scene.IsValid()) return null;
        GameObject curr = null;
        foreach (var root in scene.GetRootGameObjects()) { if (root.name == parts[1]) { curr = root; break; } }
        if (curr == null) return null;
        for (int i = 2; i < parts.Length; i++) {
            if (int.TryParse(parts[i], out int idx)) {
                if (idx < curr.transform.childCount) curr = curr.transform.GetChild(idx).gameObject; else return null;
            } else {
                Transform t = curr.transform.Find(parts[i]); if (t) curr = t.gameObject; else return null;
            }
        }
        return curr;
    }

#if UNITY_EDITOR
    // 记录场景数据
    public int RecordScene(Scene scene, int offset)
    {
        string sceneName = scene.name;
        allSceneData.RemoveAll(d => d.sceneName == sceneName);
        
        SceneLightmapData data = new SceneLightmapData { sceneName = sceneName };
        allSceneData.Add(data);

        HashSet<int> recordedIndices = new HashSet<int>();

        foreach (var root in scene.GetRootGameObjects())
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                if (r.lightmapIndex >= 0 && r.lightmapIndex < LightmapSettings.lightmaps.Length)
                {
                    int finalIndex = r.lightmapIndex + offset;

                    LightmapRendererInfo info = new LightmapRendererInfo();
                    info.objectPath = GetGameObjectPath(r.gameObject, sceneName);
                    info.finalGlobalIndex = finalIndex;
                    info.lightmapScaleOffset = r.lightmapScaleOffset;
                    data.rendererInfos.Add(info);

                    if (!recordedIndices.Contains(r.lightmapIndex))
                    {
                        recordedIndices.Add(r.lightmapIndex);
                        TextureInfo texInfo = new TextureInfo();
                        texInfo.globalIndex = finalIndex;
                        
                        LightmapData lm = LightmapSettings.lightmaps[r.lightmapIndex];
                        texInfo.texturePath = (lm.lightmapColor != null) ? AssetDatabase.GetAssetPath(lm.lightmapColor) : "";
                        
                        data.textures.Add(texInfo);
                    }
                }
            }
        }
        return LightmapSettings.lightmaps.Length;
    }

    /// <summary>
    /// 【修复】批量修正贴图路径 (针对 textures 列表)
    /// </summary>
    public void BatchFixPaths(string oldBaseDir, string newBaseDir)
    {
        string oldDirUnified = oldBaseDir.Replace("\\", "/").TrimEnd('/');
        string newDirUnified = newBaseDir.Replace("\\", "/").TrimEnd('/');

        foreach (var data in allSceneData)
        {
            // BaseScene 单独处理过了，跳过
            if (data.sceneName == "BaseScene") continue;

            // 修正 textures 列表
            foreach (var texInfo in data.textures)
            {
                string oldPath = texInfo.texturePath;
                if (string.IsNullOrEmpty(oldPath)) continue;

                // 只替换目录部分
                string fileName = System.IO.Path.GetFileName(oldPath);
                // 强制指向新的 Chunk 目录
                texInfo.texturePath = $"{newDirUnified}/{fileName}";
            }
        }
        Debug.Log($"[LightmapManager] 批量修正完成: {oldDirUnified} -> {newDirUnified}");
    }

    private string GetGameObjectPath(GameObject obj, string sceneName)
    {
        Stack<string> stack = new Stack<string>();
        Transform curr = obj.transform;
        while (curr != null) { stack.Push(curr.parent == null ? curr.name : curr.GetSiblingIndex().ToString()); curr = curr.parent; }
        return sceneName + "/" + string.Join("/", stack);
    }
#endif
}