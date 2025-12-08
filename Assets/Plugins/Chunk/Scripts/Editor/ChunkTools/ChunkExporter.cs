using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Chunk导出器类
/// 负责预览、导出和合并chunk场景
/// 修改：使用复制而非移动，保护 Prefab 实例
/// </summary>
public static class ChunkExporter
{
    /// <summary>
    /// 预览分块结构（使用复制而非移动，保护 Prefab 实例）
    /// </summary>
    public static void PreviewChunking(
    string staticNodeName,
    float chunkSize)
    {
        GameObject staticNode = GameObject.Find(staticNodeName);
        if (staticNode == null)
        {
            EditorUtility.DisplayDialog("错误", $"找不到名为'{staticNodeName}'的节点", "确定");
            return;
        }

        // 收集所有MeshRenderer节点
        List<GameObject> meshObjects = new List<GameObject>();
        ChunkToolUtility.CollectMeshRenderers(staticNode.transform, meshObjects);

        if (meshObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到需要处理的MeshRenderer节点", "确定");
            return;
        }

        // 按chunk分组
        Dictionary<Vector2Int, List<GameObject>> chunks =
                ChunkToolUtility.GroupObjectsByChunk(meshObjects, chunkSize);

        // 创建预览结构
        GameObject chunkParent = new GameObject("Chunk");

        int totalCopied = 0;
        foreach (var kvp in chunks.OrderBy(k => k.Key.x).ThenBy(k => k.Key.y))
        {
            Vector2Int chunkIndex = kvp.Key;
            List<GameObject> objects = kvp.Value;

            GameObject chunkNode = new GameObject($"Chunk_{chunkIndex.x}_{chunkIndex.y}");
            chunkNode.transform.SetParent(chunkParent.transform);

            // 复制物体而不是移动（保护 Prefab 实例）
            foreach (GameObject obj in objects)
            {
                GameObject copy = Object.Instantiate(obj);
                copy.name = obj.name; // 移除 "(Clone)" 后缀
                copy.transform.SetParent(chunkNode.transform, false);
                copy.transform.position = obj.transform.position;
                copy.transform.rotation = obj.transform.rotation;
                copy.transform.localScale = obj.transform.lossyScale;
                totalCopied++;
            }
        }

        // 隐藏原始 Static 节点
        staticNode.SetActive(false);

        EditorUtility.DisplayDialog("预览完成",
            $"已创建预览结构！\n\n" +
            $"Chunk数量: {chunks.Count}\n" +
            $"物体总数: {totalCopied}\n\n" +
            $"💡 原始节点已隐藏\n" +
            $"💡 预览使用的是复制的物体\n" +
            $"💡 查看效果后，点击'取消预览'恢复。",
            "确定");

        Debug.Log($"[预览] 已创建 {chunks.Count} 个chunk，复制了 {totalCopied} 个物体");
    }

    /// <summary>
    /// 取消预览（删除复制的物体，恢复原始节点）
    /// </summary>
    public static void CancelPreview(string staticNodeName)
    {
        GameObject chunkPreview = GameObject.Find("Chunk");
        if (chunkPreview != null && chunkPreview.transform.parent == null)
        {
            // 直接删除预览节点（因为是复制的）
            Object.DestroyImmediate(chunkPreview);
            Debug.Log("[预览] 已删除预览节点");
            
            // 恢复原始 Static 节点的显示（需要查找包括隐藏的对象）
            GameObject staticNode = FindObjectIncludingInactive(staticNodeName);
            if (staticNode != null)
            {
                staticNode.SetActive(true);
                Debug.Log("[预览] 已恢复原始 Static 节点显示");
            }
            else
            {
                Debug.LogWarning($"[预览] 未找到名为 '{staticNodeName}' 的节点");
            }
            
            EditorUtility.DisplayDialog("完成", "预览已取消，原始场景已恢复", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "未找到预览节点", "确定");
        }
    }

    /// <summary>
    /// 查找对象（包括隐藏的）
    /// </summary>
    private static GameObject FindObjectIncludingInactive(string name)
    {
        // 查找场景中所有 GameObject（包括隐藏的）
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // 只查找场景中的对象（排除 Prefab 资源）
            if (obj.scene.IsValid() && obj.name == name)
            {
                return obj;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 导出Chunk场景（使用复制而非移动，保护原场景）
    /// </summary>
    public static void ExportChunkScenes(
    string baseNodeName,
    string staticNodeName,
    float chunkSize,
    string exportPath,
    GameObject globalLightingPrefab,
    bool showDetailLog = true)
    {
        // 检查是否有预览
        GameObject chunkPreview = GameObject.Find("Chunk");
        if (chunkPreview != null && chunkPreview.transform.parent == null)
        {
            EditorUtility.DisplayDialog("警告",
                "请先点击'取消预览'再执行导出。",
                "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认导出",
                "即将导出Chunk场景！\n\n" +
                "注意：\n" +
                "• 将复制 Static 节点的物体到新场景\n" +
                "• 原场景不受影响（使用复制而非移动）\n" +
                "• 建议先保存当前场景\n" +
                "• 导出后需要单独烘焙各chunk场景\n\n" +
                "是否继续？",
                "继续", "取消"))
        {
            return;
        }

        try
        {
            Scene originalScene = EditorSceneManager.GetActiveScene();
            string originalScenePath = originalScene.path;

            if (string.IsNullOrEmpty(originalScenePath))
            {
                EditorUtility.DisplayDialog("错误", "请先保存当前场景", "确定");
                return;
            }

            // 创建导出目录
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
                AssetDatabase.Refresh();
            }

            GameObject staticNode = GameObject.Find(staticNodeName);
            GameObject baseNode = GameObject.Find(baseNodeName);

            if (staticNode == null)
            {
                EditorUtility.DisplayDialog("错误", $"找不到名为'{staticNodeName}'的节点", "确定");
                return;
            }

            // 收集Static节点下的所有MeshRenderer
            List<GameObject> staticMeshObjects = new List<GameObject>();
            ChunkToolUtility.CollectMeshRenderers(staticNode.transform, staticMeshObjects);
            
            // 合并所有需要处理的物体
            List<GameObject> allObjects = new List<GameObject>();
            allObjects.AddRange(staticMeshObjects);

            if (allObjects.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到需要处理的物体", "确定");
                return;
            }

            // 按chunk分组
            Dictionary<Vector2Int, List<GameObject>> chunks =
                    ChunkToolUtility.GroupObjectsByChunk(allObjects, chunkSize);

            int totalChunks = chunks.Count;

            Debug.Log($"[场景导出] 开始导出，共 {totalChunks} 个chunk，{allObjects.Count} 个物体（使用复制模式）");

            // 导出BaseScene
            if (baseNode != null)
            {
                ExportBaseScene(baseNode, exportPath, globalLightingPrefab);
            }

            // 导出Chunk场景
            int processedChunks = 0;
            int exportedCount = 0;
            Dictionary<Vector2Int, Bounds> chunkAABBs = new Dictionary<Vector2Int, Bounds>();
            
            foreach (var kvp in chunks.OrderBy(k => k.Key.x).ThenBy(k => k.Key.y))
            {
                Vector2Int chunkIndex = kvp.Key;
                List<GameObject> objects = kvp.Value;
                string chunkName = $"Chunk_{chunkIndex.x}_{chunkIndex.y}";

                processedChunks++;
                float progress = (float)processedChunks / (totalChunks + 1);
                EditorUtility.DisplayProgressBar("导出Chunk场景",
                    $"正在导出 {chunkName} ({processedChunks}/{totalChunks})",
                    progress);

                ChunkExportResult result = ExportSingleChunk(chunkName,
                    chunkIndex,
                    objects,
                    chunkSize,
                    exportPath,
                    globalLightingPrefab,
                    showDetailLog);

                if (result.success)
                {
                    exportedCount++;
                    chunkAABBs[chunkIndex] = result.aabb;
                }
            }

            EditorUtility.ClearProgressBar();
            
            // 更新BaseScene中的ChunkAABBManager
            UpdateChunkAABBManager(baseNode, exportPath, chunkAABBs);
            
            AssetDatabase.Refresh();

            string message = $"导出完成！\n\n" +
                    $"✅ 已导出: {exportedCount}\n" +
                    $"📊 总计: {exportedCount}/{totalChunks}\n" +
                    $"📁 路径: {exportPath}\n\n" +
                    $"💡 原场景保持不变（使用了复制模式）\n" +
                    $"💡 接下来请打开各chunk场景进行烘焙";

            EditorUtility.DisplayDialog("完成", message, "确定");
            Debug.Log($"[场景导出] {message.Replace("\n", " ")}");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", $"导出失败: {e.Message}\n\n{e.StackTrace}", "确定");
            Debug.LogError($"[场景导出] 导出失败: {e}");
        }
    }

    /// <summary>
    /// 导出BaseScene（
    /// </summary>
    private static void ExportBaseScene(
    GameObject baseNode,
    string exportPath,
    GameObject globalLightingPrefab)
    {
        try
        {
            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            newScene.name = "BaseScene";

            // 递归复制Base节点及其所有子节点，保留Prefab引用
            GameObject baseCopy = CopyGameObjectWithPrefabReferences(baseNode, newScene, null);
            baseCopy.name = baseNode.name;

            // 保存BaseScene
            string baseScenePath = Path.Combine(exportPath, "BaseScene.unity");
            EditorSceneManager.SaveScene(newScene, baseScenePath);
            
            EditorSceneManager.CloseScene(newScene, true);

            Debug.Log($"[场景导出] BaseScene 已导出: {baseScenePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[场景导出] BaseScene 导出失败: {e.Message}");
        }
    }

/// <summary>
/// 递归复制GameObject，保留Prefab引用
/// </summary>
private static GameObject CopyGameObjectWithPrefabReferences(GameObject source, Scene targetScene, Transform parent)
{
    GameObject copy;
    
    // 检查是否是Prefab实例的根（最外层Prefab）
    if (PrefabUtility.IsAnyPrefabInstanceRoot(source))
    {
        // 是Prefab实例的根，使用InstantiatePrefab保留引用
        GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(source);
        if (prefabSource != null)
        {
            copy = (GameObject)PrefabUtility.InstantiatePrefab(prefabSource, targetScene);
            
            // 复制transform属性
            copy.transform.position = source.transform.position;
            copy.transform.rotation = source.transform.rotation;
            copy.transform.localScale = source.transform.localScale;
            
            // 只有当parent不为null时才设置父节点
            if (parent != null)
            {
                copy.transform.SetParent(parent, true);
            }
            
            Debug.Log($"[BaseScene导出] 保留Prefab引用: {source.name}");
        }
        else
        {
            // 降级：无法获取Prefab源，使用普通复制
            copy = Object.Instantiate(source);
            
            // ⚠️ 先移动到场景，再设置parent
            SceneManager.MoveGameObjectToScene(copy, targetScene);
            
            if (parent != null)
            {
                copy.transform.SetParent(parent, true);
            }
        }
    }
    else
    {
        // 不是Prefab根，创建空物体并递归复制子节点
        copy = new GameObject(source.name);
        copy.transform.position = source.transform.position;
        copy.transform.rotation = source.transform.rotation;
        copy.transform.localScale = source.transform.localScale;
        
        // ⚠️ 关键修复：先移动到场景（此时是root），再设置parent
        SceneManager.MoveGameObjectToScene(copy, targetScene);
        
        // 然后才设置父节点
        if (parent != null)
        {
            copy.transform.SetParent(parent, true);
        }
        
        // 复制组件（除了Transform）
        Component[] components = source.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp is Transform) continue;
            
            try
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(comp);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(copy);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BaseScene导出] 复制组件失败: {comp.GetType().Name}, 错误: {ex.Message}");
            }
        }
        
        // 递归复制子节点
        foreach (Transform child in source.transform)
        {
            CopyGameObjectWithPrefabReferences(child.gameObject, targetScene, copy.transform);
        }
    }
    
    return copy;
}

    /// <summary>
    /// Chunk 导出结果
    /// </summary>
    private class ChunkExportResult
    {
        public bool success;
        public Bounds aabb;
        
        public ChunkExportResult(bool success, Bounds aabb = default)
        {
            this.success = success;
            this.aabb = aabb;
        }
    }

    /// <summary>
    /// 导出单个Chunk场景
    /// </summary>
    private static ChunkExportResult ExportSingleChunk(
    string chunkName,
    Vector2Int chunkIndex,
    List<GameObject> objects,
    float chunkSize,
    string exportPath,
    GameObject globalLightingPrefab,
    bool showDetailLog)
    {
        try
        {
            
            // 检查是否已存在场景
            string scenePath = Path.Combine(exportPath, $"{chunkName}.unity");
            
            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            newScene.name = chunkName;

            // 创建chunk根节点
            GameObject chunkRoot = new GameObject(chunkName);
            SceneManager.MoveGameObjectToScene(chunkRoot, newScene);
            

            // 复制物体到新场景（而不是移动）
            int copiedCount = 0;
            List<Renderer> allRenderers = new List<Renderer>();
            
            foreach (GameObject obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }
                
                try
                {
                    // 复制物体
                    GameObject objCopy = Object.Instantiate(obj);
                    objCopy.name = obj.name; // 移除 "(Clone)" 后缀
                    
                    // 保持世界空间的位置、旋转和缩放
                    objCopy.transform.position = obj.transform.position;
                    objCopy.transform.rotation = obj.transform.rotation;
                    objCopy.transform.localScale = obj.transform.lossyScale;
                    
                    // 移动到新场景并设置父节点
                    SceneManager.MoveGameObjectToScene(objCopy, newScene);
                    objCopy.transform.SetParent(chunkRoot.transform, true);
                    
                    // 收集所有Renderer用于计算高度
                    allRenderers.AddRange(objCopy.GetComponentsInChildren<Renderer>());
                    
                    copiedCount++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[场景导出] {chunkName} 复制物体失败: {obj.name}, 错误: {ex.Message}");
                }
            }

            // 计算AABB
            // XZ范围：使用chunk的理论范围（基于chunkSize和chunkIndex）
            float minX = chunkIndex.x * chunkSize;
            float maxX = (chunkIndex.x + 1) * chunkSize;
            float minZ = chunkIndex.y * chunkSize;
            float maxZ = (chunkIndex.y + 1) * chunkSize;
            
            // Y范围：遍历所有Renderer计算最小最大高度
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            bool hasRenderer = false;
            
            foreach (var renderer in allRenderers)
            {
                if (renderer != null)
                {
                    Bounds b = renderer.bounds;
                    minY = Mathf.Min(minY, b.min.y);
                    maxY = Mathf.Max(maxY, b.max.y);
                    hasRenderer = true;
                }
            }
            
            // 如果没有Renderer，使用默认高度
            if (!hasRenderer)
            {
                minY = 0;
                maxY = 10;
                Debug.LogWarning($"[场景导出] {chunkName} 没有Renderer,使用默认高度");
            }
            
            // 构建最终AABB
            Vector3 center = new Vector3(
                (minX + maxX) * 0.5f,
                (minY + maxY) * 0.5f,
                (minZ + maxZ) * 0.5f
            );
            Vector3 size = new Vector3(
                maxX - minX,
                maxY - minY,
                maxZ - minZ
            );
            Bounds totalBounds = new Bounds(center, size);

            Debug.Log($"[场景导出] {chunkName} 复制了 {copiedCount} 个物体, AABB: Center={totalBounds.center}, Size={totalBounds.size}");

            // 保存场景
            EditorSceneManager.MarkSceneDirty(newScene);
            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);

            if (saved)
            {
                if (showDetailLog)
                {
                    Debug.Log($"[场景导出] {chunkName} 保存成功: {scenePath}");
                }
            }
            else
            {
                Debug.LogError($"[场景导出] {chunkName} 保存失败！");
            }

            // 关闭新场景
            EditorSceneManager.CloseScene(newScene, true);

            return new ChunkExportResult(saved, totalBounds);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[场景导出] {chunkName} 导出失败: {e.Message}");
            return new ChunkExportResult(false);
        }
    }

  

    /// <summary>
    /// 递归收集物体及其所有子节点
    /// </summary>
    private static void CollectObjectsRecursive(GameObject obj, List<GameObject> list)
    {
        list.Add(obj);
        
        foreach (Transform child in obj.transform)
        {
            CollectObjectsRecursive(child.gameObject, list);
        }
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    private static string GetRelativePath(Transform root, Transform target)
    {
        string path = target.name;
        Transform current = target.parent;
        
        while (current != null && current != root)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }

    /// <summary>
    /// 打开工作场景（BaseScene）
    /// </summary>
    public static void OpenWorkScene(string exportPath, GameObject globalLightingPrefab)
    {
        string baseScenePath = Path.Combine(exportPath, "BaseScene.unity");

        if (!File.Exists(baseScenePath))
        {
            if (!EditorUtility.DisplayDialog("BaseScene 不存在",
                    "BaseScene.unity 不存在！\n\n" +
                    "是否创建一个空的工作场景？",
                    "创建", "取消"))
            {
                return;
            }

            // 创建空的工作场景
            Scene workScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建 EditPoint
            GameObject editPoint = new GameObject("EditPoint");
            editPoint.transform.position = Vector3.zero;

            // 添加一个图标（方便在 Scene 视图中看到）
            editPoint.AddComponent<EditPointMarker>();

            // 保存为 BaseScene
            EditorSceneManager.SaveScene(workScene, baseScenePath);

            EditorUtility.DisplayDialog("完成",
                $"工作场景已创建！\n\n" +
                $"位置: {baseScenePath}\n\n" +
                $"✅ 已创建 EditPoint 节点\n" +
                $"💡 将 EditPoint 移动到要编辑的位置\n" +
                $"💡 然后点击'加载周围Chunk'",
                "确定");

            Debug.Log($"[工作场景] 已创建: {baseScenePath}");
            return;
        }

        // 打开 BaseScene
        Scene baseScene = EditorSceneManager.OpenScene(baseScenePath, OpenSceneMode.Single);

        // 检查是否有 EditPoint
        GameObject existingEditPoint = GameObject.Find("EditPoint");
        if (existingEditPoint == null)
        {
            // 创建 EditPoint
            GameObject editPoint = new GameObject("EditPoint");
            editPoint.transform.position = Vector3.zero;
            editPoint.AddComponent<EditPointMarker>();

            EditorSceneManager.MarkSceneDirty(baseScene);

            EditorUtility.DisplayDialog("提示",
                "已自动创建 EditPoint 节点！\n\n" +
                "💡 将它移动到要编辑的位置\n" +
                "💡 然后点击'加载周围Chunk'",
                "确定");
        }

        // 选中 EditPoint
        if (existingEditPoint != null)
        {
            Selection.activeGameObject = existingEditPoint;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        Debug.Log($"[工作场景] 已打开 BaseScene");
    }

    /// <summary>
    /// 加载 EditPoint 周围的 Chunk 场景
    /// </summary>
    public static void LoadChunksAroundEditPoint(
    string exportPath,
    float chunkSize,
    int radius)
    {
        // 查找 EditPoint
        GameObject editPoint = GameObject.Find("EditPoint");
        if (editPoint == null)
        {
            EditorUtility.DisplayDialog("错误",
                "未找到 EditPoint 节点！\n\n" +
                "请先在场景中创建名为 'EditPoint' 的物体。",
                "确定");
            return;
        }

        // 计算 EditPoint 所在的 chunk 索引
        Vector3 editPos = editPoint.transform.position;
        Vector2Int centerChunk = ChunkToolUtility.CalculateChunkIndex(editPos, chunkSize);

        // 计算需要加载的 chunk 列表
        List<Vector2Int> chunksToLoad = new List<Vector2Int>();
        for (int x = centerChunk.x - radius; x <= centerChunk.x + radius; x++)
        {
            for (int z = centerChunk.y - radius; z <= centerChunk.y + radius; z++)
            {
                chunksToLoad.Add(new Vector2Int(x, z));
            }
        }

        // 确认对话框
        if (!EditorUtility.DisplayDialog("确认加载",
                $"将加载 EditPoint 周围的 chunk：\n\n" +
                $"• EditPoint 位置: {editPos}\n" +
                $"• 中心 Chunk: Chunk_{centerChunk.x}_{centerChunk.y}\n" +
                $"• 加载半径: {radius}\n" +
                $"• Chunk 数量: {chunksToLoad.Count}\n\n" +
                $"是否继续？",
                "加载", "取消"))
        {
            return;
        }

        // 先卸载已加载的 chunk（保留 BaseScene）
        UnloadAllChunksExceptBase();

        // 加载新的 chunk
        int loadedCount = 0;
        int failedCount = 0;

        for (int i = 0; i < chunksToLoad.Count; i++)
        {
            Vector2Int chunkIndex = chunksToLoad[i];
            string chunkName = $"Chunk_{chunkIndex.x}_{chunkIndex.y}";
            string chunkPath = Path.Combine(exportPath, $"{chunkName}.unity");

            float progress = (float)i / chunksToLoad.Count;
            EditorUtility.DisplayProgressBar("加载 Chunk",
                $"正在加载 {chunkName} ({i + 1}/{chunksToLoad.Count})",
                progress);

            if (File.Exists(chunkPath))
            {
                try
                {
                    EditorSceneManager.OpenScene(chunkPath, OpenSceneMode.Additive);
                    loadedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[加载Chunk] {chunkName} 加载失败: {e.Message}");
                    failedCount++;
                }
            }
            else
            {
                // chunk 不存在（可能是边界外）
                failedCount++;
            }
        }

        EditorUtility.ClearProgressBar();

        string message = $"Chunk 加载完成！\n\n" +
                $"✅ 成功加载: {loadedCount}\n" +
                $"⚠️ 未找到: {failedCount}\n" +
                $"📍 中心: Chunk_{centerChunk.x}_{centerChunk.y}\n\n" +
                $"💡 现在可以编辑，修改后记得保存对应场景。";

        EditorUtility.DisplayDialog("完成", message, "开始编辑");

        Debug.Log($"[加载Chunk] 已加载 {loadedCount} 个 chunk，中心: Chunk_{centerChunk.x}_{centerChunk.y}");
    }

    /// <summary>
    /// 卸载所有 Chunk 场景，保留 BaseScene
    /// </summary>
    public static void UnloadAllChunksExceptBase()
    {
        List<Scene> scenesToUnload = new List<Scene>();

        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);

            // 只卸载 chunk 场景，保留 BaseScene 和其他场景
            if (scene.name.StartsWith("Chunk_"))
            {
                scenesToUnload.Add(scene);
            }
        }

        foreach (Scene scene in scenesToUnload)
        {
            EditorSceneManager.CloseScene(scene, false);
        }

        if (scenesToUnload.Count > 0)
        {
            Debug.Log($"[工作场景] 已卸载 {scenesToUnload.Count} 个 chunk");
        }
    }
    
    /// <summary>
    /// 更新BaseScene中的ChunkAABBManager
    /// </summary>
    private static void UpdateChunkAABBManager(GameObject baseNode, string exportPath, Dictionary<Vector2Int, Bounds> chunkAABBs)
    {
        if (chunkAABBs == null || chunkAABBs.Count == 0)
        {
            Debug.LogWarning("[AABB] 没有AABB数据需要更新");
            return;
        }
        
        try
        {
            string baseScenePath = Path.Combine(exportPath, "BaseScene.unity");
            if (!File.Exists(baseScenePath))
            {
                Debug.LogWarning("[AABB] BaseScene不存在，跳过AABB更新");
                return;
            }
            
            // 打开BaseScene
            Scene baseScene = EditorSceneManager.OpenScene(baseScenePath, OpenSceneMode.Additive);
            
            // 在Base节点下查找或创建ChunkAABB节点
            GameObject baseInScene = null;
            foreach (var root in baseScene.GetRootGameObjects())
            {
                if (root.name == (baseNode != null ? baseNode.name : "Base"))
                {
                    baseInScene = root;
                    break;
                }
            }
            
            if (baseInScene == null)
            {
                // 如果没有Base节点，在根创建ChunkAABB
                baseInScene = new GameObject("ChunkAABB");
                SceneManager.MoveGameObjectToScene(baseInScene, baseScene);
            }
            
            // 查找ChunkAABB节点
            Transform chunkAABBTransform = baseInScene.transform.Find("ChunkAABB");
            GameObject chunkAABBObj;
            ChunkAABBManager manager;
            
            if (chunkAABBTransform == null)
            {
                // 创建ChunkAABB节点
                chunkAABBObj = new GameObject("ChunkAABB");
                chunkAABBObj.transform.SetParent(baseInScene.transform);
                chunkAABBObj.transform.localPosition = Vector3.zero;
                manager = chunkAABBObj.AddComponent<ChunkAABBManager>();
                Debug.Log("[AABB] 已创建ChunkAABB节点");
            }
            else
            {
                chunkAABBObj = chunkAABBTransform.gameObject;
                manager = chunkAABBObj.GetComponent<ChunkAABBManager>();
                if (manager == null)
                {
                    manager = chunkAABBObj.AddComponent<ChunkAABBManager>();
                }
                Debug.Log("[AABB] 使用现有ChunkAABB节点");
            }
            
            // 更新AABB数据
            manager.Clear();
            foreach (var kvp in chunkAABBs)
            {
                string chunkName = $"Chunk_{kvp.Key.x}_{kvp.Key.y}";
                manager.AddOrUpdateChunkAABB(chunkName, kvp.Key, kvp.Value);
            }
            
            // 保存场景
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(baseScene);
            EditorSceneManager.SaveScene(baseScene);
            
            Debug.Log($"[AABB] 已更新 {chunkAABBs.Count} 个Chunk的AABB数据到BaseScene");
            
            // 关闭BaseScene（如果原本没打开）
            EditorSceneManager.CloseScene(baseScene, false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AABB] 更新ChunkAABBManager失败: {e.Message}\n{e.StackTrace}");
        }
    }
}