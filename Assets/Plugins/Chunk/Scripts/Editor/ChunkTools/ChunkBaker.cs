using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class ChunkBaker
{
    private static bool isBaking = false;
    private static bool isFinalBake = false;
    private static string exportPath = "";
    private static string lightmapOutputPath = "";
    private static GameObject globalLightingPrefab = null;
    private static string[] chunkFilesToBake = null;
    private static LightmapManager lightmapManager = null;

    public static void BakeAll(string chunksExportPath, GameObject lightingPrefab, bool isFinal, string outputPath)
    {
        if (isBaking) return;
        if (!Directory.Exists(chunksExportPath)) return;

        chunkFilesToBake = Directory.GetFiles(chunksExportPath, "Chunk_*.unity");
        System.Array.Sort(chunkFilesToBake);

        exportPath = chunksExportPath;
        lightmapOutputPath = outputPath;
        globalLightingPrefab = lightingPrefab;
        isFinalBake = isFinal;
        isBaking = true;

        BakeBaseSceneInternal();
    }

    // --- BaseScene 逻辑 (保持不变) ---
    private static void BakeBaseSceneInternal()
    {
        try {
            string path = Path.Combine(exportPath, "BaseScene.unity");
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            SetupLightmapManager();
            LightingConfigHelper.ApplyGlobalLighting(globalLightingPrefab, isFinalBake, true);
            foreach(var r in scene.GetRootGameObjects()) 
                if(!r.name.StartsWith("Global") && r.name!="LightmapManager") 
                    ChunkToolUtility.SetStaticRecursively(r, ChunkToolUtility.AllStaticFlags);
            EditorSceneManager.SaveScene(scene);
            Lightmapping.bakeCompleted += OnBaseSceneBakeCompleted;
            Lightmapping.BakeAsync();
        } catch { isBaking = false; }
    }

    private static void OnBaseSceneBakeCompleted()
    {
        Lightmapping.bakeCompleted -= OnBaseSceneBakeCompleted;
        try {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (lightmapManager != null) lightmapManager.RecordScene(scene, 0);
            
            // BaseScene 单独处理移动
            string targetDir = MoveFilesFromDir(Path.Combine(exportPath, scene.name), "BaseScene");
            if (lightmapManager != null && !string.IsNullOrEmpty(targetDir)) FixRecordedPathsForSingleScene(scene.name, targetDir);

            ClearSceneLightmapReferences(scene);
            LightmapSettings.lightmaps = new LightmapData[0];
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene);

            BakeChunksInternal();
        } catch { isBaking = false; }
    }

    // --- Chunk 逻辑 (核心修改) ---
    private static void BakeChunksInternal()
    {
        try {
            // 加载第一个 Chunk 作为 Active Scene
            Scene first = EditorSceneManager.OpenScene(chunkFilesToBake[0], OpenSceneMode.Single);
            for(int i=1; i<chunkFilesToBake.Length; i++) EditorSceneManager.OpenScene(chunkFilesToBake[i], OpenSceneMode.Additive);
            
            LightingConfigHelper.ApplyGlobalLighting(globalLightingPrefab, isFinalBake, false);
            for(int i=0; i<EditorSceneManager.sceneCount; i++) {
                var s = EditorSceneManager.GetSceneAt(i);
                foreach(var r in s.GetRootGameObjects()) if(r.name.StartsWith("Chunk_")) ChunkToolUtility.SetStaticRecursively(r, ChunkToolUtility.AllStaticFlags);
            }
            EditorSceneManager.SaveOpenScenes();
            Lightmapping.bakeCompleted += OnChunksBakeCompleted;
            Lightmapping.BakeAsync();
        } catch { isBaking = false; }
    }

    private static void OnChunksBakeCompleted()
    {
        Lightmapping.bakeCompleted -= OnChunksBakeCompleted;

        try
        {
            LightingConfigHelper.RemoveGlobalLightingFromScene();

            // 1. 准备环境
            string basePath = Path.Combine(exportPath, "BaseScene.unity");
            Scene baseScene = EditorSceneManager.OpenScene(basePath, OpenSceneMode.Additive);
            SetupLightmapManager();

            // 2. 计算 Offset (读取 BaseScene 占位)
            int chunkOffset = 0;
            if (lightmapManager != null)
            {
                var baseData = lightmapManager.allSceneData.Find(d => d.sceneName == "BaseScene");
                if (baseData != null)
                {
                    int maxBaseIndex = -1;
                    foreach(var t in baseData.textures) if(t.globalIndex > maxBaseIndex) maxBaseIndex = t.globalIndex;
                    chunkOffset = maxBaseIndex + 1;
                }
            }

            // 3. 收集所有 Chunk 场景
            List<Scene> chunks = new List<Scene>();
            for(int i=0; i<EditorSceneManager.sceneCount; i++) {
                Scene s = EditorSceneManager.GetSceneAt(i);
                if(s.name.StartsWith("Chunk_")) chunks.Add(s);
            }
            chunks.Sort((a,b)=>a.name.CompareTo(b.name));

            // =================================================================
            // 步骤 A: 仅记录！(绝不移动文件)
            // =================================================================
            foreach(Scene s in chunks)
            {
                if (lightmapManager != null)
                {
                    // 记录时，文件还在原处，路径是有效的
                    lightmapManager.RecordScene(s, chunkOffset);
                }
            }

            // =================================================================
            // 步骤 B: 统一移动文件
            // 注意：Additive烘焙的所有贴图都在【Active Scene】的文件夹里！
            // 也就是 chunks[0] (第一个加载的场景) 的目录
            // =================================================================
            string activeSceneName = SceneManager.GetActiveScene().name; // 通常是 Chunk_0_0
            string sourceDir = Path.Combine(exportPath, activeSceneName);
            
            // 移动所有贴图到 Output/Chunk 目录
            string finalTargetDir = MoveFilesFromDir(sourceDir, "Chunk");

            // =================================================================
            // 步骤 C: 批量修正 Manager 里的路径
            // =================================================================
            if (lightmapManager != null && !string.IsNullOrEmpty(finalTargetDir))
            {
                // 把刚才记录的所有 Chunk 路径 (它们指向 sourceDir)，批量改成 finalTargetDir
                // 获取相对 Project 的路径用于替换
                string projectPath = Path.GetDirectoryName(Application.dataPath).Replace("\\", "/");
                string relativeTargetDir = finalTargetDir.Replace("\\", "/").Replace(projectPath, "").TrimStart('/');
                
                // 源目录的相对路径
                string relativeSourceDir = sourceDir.Replace("\\", "/").Replace(projectPath, "").TrimStart('/');

                // 调用 Manager 的批量修正
                lightmapManager.BatchFixPaths(relativeSourceDir, relativeTargetDir);
                
                EditorUtility.SetDirty(lightmapManager);
            }

            // =================================================================
            // 4. 收尾保存
            // =================================================================
            AssetDatabase.Refresh();
            if (lightmapManager != null) lightmapManager.UnloadLightmap();

            List<Scene> scenesToSave = new List<Scene>(chunks);
            scenesToSave.Add(baseScene);
            foreach(var s in scenesToSave) if(s.isLoaded) EditorSceneManager.SaveScene(s);

            isBaking = false;
            EditorUtility.DisplayDialog("完成", "烘焙完成！", "OK");
        }
        catch (System.Exception e) { 
            isBaking = false;
            Debug.LogError(e); 
            EditorUtility.DisplayDialog("错误", e.Message, "OK");
        }
    }

    // --- 辅助方法 ---

    private static string MoveFilesFromDir(string sourceDir, string subFolder)
    {
        if(string.IsNullOrEmpty(lightmapOutputPath)) return null;

        // 兼容性检查：如果 sourceDir 不存在，尝试在同级目录找
        if (!Directory.Exists(sourceDir))
        {
            string altDir = Path.Combine(Path.GetDirectoryName(chunkFilesToBake[0]), Path.GetFileName(sourceDir));
            if (Directory.Exists(altDir)) sourceDir = altDir;
            else return null;
        }

        string targetDir = Path.Combine(lightmapOutputPath, subFolder);
        if(!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        Debug.Log($"[ChunkBaker] 移动文件: {sourceDir} -> {targetDir}");

        foreach(var f in Directory.GetFiles(sourceDir)) {
            string n = Path.GetFileName(f);
            // 排除 LightingData 和 meta
            if(n.Contains("LightingData") || n.EndsWith(".meta")) { 
                if(n.Contains("LightingData")) File.Delete(f); 
                continue; 
            }
            
            string dest = Path.Combine(targetDir, n);
            if(File.Exists(dest)) File.Delete(dest);
            File.Move(f, dest);
        }
        return targetDir;
    }

    private static void SetupLightmapManager() {
        GameObject go = GameObject.Find("LightmapManager");
        if (!go) { go = new GameObject("LightmapManager"); lightmapManager = go.AddComponent<LightmapManager>(); }
        else { lightmapManager = go.GetComponent<LightmapManager>(); if(!lightmapManager) lightmapManager = go.AddComponent<LightmapManager>(); }
    }
    
    private static void ClearSceneLightmapReferences(Scene s) {
        foreach(var r in s.GetRootGameObjects()) foreach(var m in r.GetComponentsInChildren<MeshRenderer>()) 
            if(m.lightmapIndex>=0) { m.lightmapIndex=-1; m.lightmapScaleOffset=Vector4.zero; }
    }

    private static void FixRecordedPathsForSingleScene(string sceneName, string targetDirAbsPath)
    {
        // 仅用于 BaseScene 的单次修正
        var data = lightmapManager.allSceneData.Find(d => d.sceneName == sceneName);
        if (data == null) return;
        string projectPath = Path.GetDirectoryName(Application.dataPath).Replace("\\", "/");
        string relDir = targetDirAbsPath.Replace("\\", "/").Replace(projectPath, "").TrimStart('/');
        foreach(var tex in data.textures) {
            if(!string.IsNullOrEmpty(tex.texturePath))
                tex.texturePath = $"{relDir}/{Path.GetFileName(tex.texturePath)}";
        }
    }
}