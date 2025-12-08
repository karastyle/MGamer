using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using EasyTools;

/// <summary>
/// 场景Chunk流式加载管理器 - 基于视锥体裁剪优化版
/// </summary>
public class ChunkStreamingLoader : MonoBehaviour
{
    [Header("分块设置")]
    [Tooltip("分块大小(米)")]
    public float chunkSize = 100f;
    
    [Tooltip("加载半径(以Chunk为单位) - 作为最大检测范围")]
    [Range(1, 10)]
    public int maxCheckRadius = 5;
    
    [Tooltip("卸载半径(应该大于视野范围,避免频繁加载卸载)")]
    [Range(1, 15)]
    public int unloadRadius = 8;
    
    [Header("摄像机设置")]
    [Tooltip("主摄像机，留空则自动查找MainCamera")]
    public Camera mainCamera;
    
    [Tooltip("ChunkAABB管理器，留空则自动查找")]
    public ChunkAABBManager aabbManager;
    
    [Header("场景检查")]
    [Tooltip("是否在初始化时预检查场景")]
    public bool preCheckScenes = true;
    
    [Header("性能优化")]
    [Tooltip("卸载延迟时间(秒),避免频繁卸载")]
    public float unloadDelayTime = 3f;
    
    [Tooltip("每帧最多加载的chunk数量")]
    public int maxLoadPerFrame = 2;
    
    [Tooltip("每帧最多卸载的chunk数量")]
    public int maxUnloadPerFrame = 1;
    
    [Tooltip("视锥体检测更新频率(秒)")]
    public float frustumCheckInterval = 0.1f;
    
    [Header("场景设置")]
    [Tooltip("Chunk场景资源路径前缀")]
    public string chunkScenePrefix = "Chunk_";

    public string chunkScenePath = "";
    
    [Header("调试")]
    public bool showDebugInfo = false;
    public bool enableGizmos = false;
    
    public static bool IsInitialized { get; private set; } = false;
    
    private Vector2Int currentChunkIndex;
    private Vector3 lastPosition;
    private Quaternion lastCameraRotation;
    private float lastFrustumCheckTime;
    
    // 摄像机平截头体平面
    private Plane[] frustumPlanes;
    
    // 场景有效性缓存
    private HashSet<Vector2Int> validChunks = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> invalidChunks = new HashSet<Vector2Int>();
    
    // 已加载的chunk字典
    private Dictionary<Vector2Int, SceneHandle> loadedChunks = new Dictionary<Vector2Int, SceneHandle>();
    
    // 正在加载的chunk集合
    private HashSet<Vector2Int> loadingChunks = new HashSet<Vector2Int>();
    
    // 待卸载的chunk字典 <ChunkIndex, 开始等待卸载的时间>
    private Dictionary<Vector2Int, float> chunksToUnload = new Dictionary<Vector2Int, float>();
    
    // 加载队列
    private Queue<Vector2Int> loadQueue = new Queue<Vector2Int>();
    
    // 卸载队列
    private Queue<Vector2Int> unloadQueue = new Queue<Vector2Int>();
    
    // 当前在视锥体内的chunk
    private HashSet<Vector2Int> chunksInFrustum = new HashSet<Vector2Int>();

    public void Start()
    {
        StartCoroutine(Initialize());
    }

    public IEnumerator Initialize()
    {
        // 获取主摄像机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[ChunkStreaming] 未找到主摄像机！");
                yield break;
            }
        }
        
        // 查找ChunkAABBManager
        if (aabbManager == null)
        {
            aabbManager = FindObjectOfType<ChunkAABBManager>();
            if (aabbManager == null)
            {
                Debug.LogError("[ChunkStreaming] 未找到ChunkAABBManager！");
                yield break;
            }
        }
        
        IsInitialized = true;
        
        currentChunkIndex = GetChunkIndex(mainCamera.transform.position);
        lastPosition = mainCamera.transform.position;
        lastCameraRotation = mainCamera.transform.rotation;
        lastFrustumCheckTime = Time.time;
        
        // 预检查场景是否存在
        if (preCheckScenes)
        {
            yield return PreCheckChunkScenes();
        }
        
        UpdateChunks();
        
        Debug.Log($"[ChunkStreaming] 初始化完成,当前Chunk: {currentChunkIndex}, " +
                  $"有效场景: {validChunks.Count}个, 无效场景: {invalidChunks.Count}个");
        
        // 启动加载卸载协程
        StartCoroutine(ProcessLoadQueue());
        StartCoroutine(ProcessUnloadQueue());
        StartCoroutine(CheckDelayedUnload());
        
        yield return null;
    }
    
    /// <summary>
    /// 预检查周围chunk场景是否存在
    /// 修改：不再检查文件，直接将 ChunkAABBManager 中的所有 Chunk 视为有效
    /// </summary>
    private IEnumerator PreCheckChunkScenes()
    {
        Debug.Log($"[ChunkStreaming] 开始从 AABBManager 初始化有效场景列表");
        
        validChunks.Clear();
        invalidChunks.Clear();

        if (aabbManager != null && aabbManager.chunkAABBs != null)
        {
            // 将 AABBManager 中记录的所有 Chunk 加入有效列表
            foreach (var data in aabbManager.chunkAABBs)
            {
                validChunks.Add(data.chunkIndex);
            }
        }
        else
        {
            Debug.LogError("[ChunkStreaming] AABBManager 为空或无数据！");
        }
        
        Debug.Log($"[ChunkStreaming] 初始化完成: AABB记录中共有 {validChunks.Count} 个有效Chunk");
        
        yield return null;
    }

    /// <summary>
    /// 检查单个chunk场景是否存在
    /// 修改：根据 chunkIndex 是否存在于 aabbManager.chunkAABBs 中来判断
    /// </summary>
    private bool IsChunkSceneValid(Vector2Int chunkIndex)
    {
        // 1. 如果在有效缓存中，直接返回 true (由 PreCheck 填充)
        if (validChunks.Contains(chunkIndex))
        {
            return true;
        }

        // 2. 如果不在缓存中，为了保险起见，再次去 aabbManager 列表中查找
        // (防止运行时动态添加了AABB但没更新缓存的情况)
        if (aabbManager != null && aabbManager.chunkAABBs != null)
        {
            // 注意：List.Exists 会产生少量 GC，但通常只有边缘情况会走到这里
            bool exists = aabbManager.chunkAABBs.Exists(x => x.chunkIndex == chunkIndex);
            
            if (exists)
            {
                validChunks.Add(chunkIndex); // 补录到缓存
                return true;
            }
        }
        
        // 既不在缓存，也不在 AABB 列表中，视为无效
        return false;
    }

    /// <summary>
    /// 获取场景名称
    /// </summary>
    private string GetSceneName(Vector2Int chunkIndex)
    {
        if (Application.isEditor)
        {
            return $"{chunkScenePath}/{chunkScenePrefix}{chunkIndex.x}_{chunkIndex.y}.unity";
        }
        else
        {
            return $"{chunkScenePrefix}{chunkIndex.x}_{chunkIndex.y}";
        }
    }

    private void Update()
    {
        if (!IsInitialized) return;
        
        // 使用相机位置
        Vector3 currentPosition = mainCamera.transform.position;
        Vector3 movement = currentPosition - lastPosition;
        lastPosition = currentPosition;
        
        // 计算相机位置对应的chunk索引
        Vector2Int newChunkIndex = GetChunkIndex(currentPosition);
        
        // 检测相机旋转
        Quaternion currentRotation = mainCamera.transform.rotation;
        float rotationDiff = Quaternion.Angle(lastCameraRotation, currentRotation);
        bool cameraRotated = rotationDiff > 1f; // 旋转超过1度就更新
        
        if (cameraRotated)
        {
            lastCameraRotation = currentRotation;
        }
        
        // 定期更新视锥体检测
        bool shouldUpdate = false;
        if (Time.time - lastFrustumCheckTime >= frustumCheckInterval)
        {
            lastFrustumCheckTime = Time.time;
            shouldUpdate = true;
        }
        
        // 如果chunk索引发生变化、相机旋转或者到了检测时间
        if (newChunkIndex != currentChunkIndex)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ChunkStreaming] Chunk变化: {currentChunkIndex} -> {newChunkIndex}");
            }
            
            currentChunkIndex = newChunkIndex;
            shouldUpdate = true;
        }
        else if (cameraRotated)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ChunkStreaming] 相机旋转,更新视锥体检测");
            }
            shouldUpdate = true;
        }
        
        if (shouldUpdate)
        {
            UpdateChunks();
        }
    }

    private Vector2Int GetChunkIndex(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / chunkSize),
            Mathf.FloorToInt(worldPosition.z / chunkSize)
        );
    }

    /// <summary>
    /// 更新需要加载和卸载的chunk - 基于视锥体检测 + unloadRadius范围
    /// </summary>
    private void UpdateChunks()
    {
        // 更新摄像机视锥体平面
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        
        // 清空上次的结果
        chunksInFrustum.Clear();
        
        // 应该加载的chunk集合
        HashSet<Vector2Int> shouldLoadChunks = new HashSet<Vector2Int>();
        
        // 1. 遍历范围内的所有chunk，检测是否在视锥体内
        for (int x = -maxCheckRadius; x <= maxCheckRadius; x++)
        {
            for (int z = -maxCheckRadius; z <= maxCheckRadius; z++)
            {
                Vector2Int chunkIndex = currentChunkIndex + new Vector2Int(x, z);
                
                // 先检查场景是否存在
                if (!IsChunkSceneValid(chunkIndex))
                {
                    continue;
                }
                
                // 从AABBManager获取该chunk的AABB
                Bounds chunkBounds = GetChunkBounds(chunkIndex);
                
                // 检测AABB是否与视锥体相交
                if (GeometryUtility.TestPlanesAABB(frustumPlanes, chunkBounds))
                {
                    chunksInFrustum.Add(chunkIndex);
                    shouldLoadChunks.Add(chunkIndex);
                }
            }
        }
        
        // 2. unloadRadius范围内的chunk也要加载
        for (int x = -unloadRadius; x <= unloadRadius; x++)
        {
            for (int z = -unloadRadius; z <= unloadRadius; z++)
            {
                Vector2Int chunkIndex = currentChunkIndex + new Vector2Int(x, z);
                
                // 检查场景是否存在
                if (!IsChunkSceneValid(chunkIndex))
                {
                    continue;
                }
                
                shouldLoadChunks.Add(chunkIndex);
            }
        }
        
        // 3. 处理需要加载的chunk
        foreach (var chunkIndex in shouldLoadChunks)
        {
            // 如果在待卸载列表中,移除
            if (chunksToUnload.ContainsKey(chunkIndex))
            {
                chunksToUnload.Remove(chunkIndex);
            }
            
            // 如果未加载且不在加载中,添加到加载队列
            if (!loadedChunks.ContainsKey(chunkIndex) && 
                !loadingChunks.Contains(chunkIndex) && 
                !loadQueue.Contains(chunkIndex))
            {
                loadQueue.Enqueue(chunkIndex);
            }
        }
        
        // 4. 找出应该卸载的chunk - 超出unloadRadius范围的
        HashSet<Vector2Int> shouldUnloadChunks = new HashSet<Vector2Int>();
        foreach (var chunkIndex in loadedChunks.Keys)
        {
            // 计算距离
            int distance = Mathf.Max(
                Mathf.Abs(chunkIndex.x - currentChunkIndex.x),
                Mathf.Abs(chunkIndex.y - currentChunkIndex.y)
            );
            
            if (distance > unloadRadius)
            {
                shouldUnloadChunks.Add(chunkIndex);
            }
        }
        
        // 添加到延迟卸载字典
        foreach (var chunkIndex in shouldUnloadChunks)
        {
            if (!chunksToUnload.ContainsKey(chunkIndex))
            {
                chunksToUnload[chunkIndex] = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 获取Chunk的AABB包围盒
    /// </summary>
    private Bounds GetChunkBounds(Vector2Int chunkIndex)
    {
        if (aabbManager != null && aabbManager.chunkAABBs != null)
        {
            // 从AABBManager查找对应的AABB
            foreach (var data in aabbManager.chunkAABBs)
            {
                if (data.chunkIndex == chunkIndex)
                {
                    return data.bounds;
                }
            }
        }
        
        // 如果没找到，使用默认AABB（基于chunkSize）
        float minX = chunkIndex.x * chunkSize;
        float maxX = (chunkIndex.x + 1) * chunkSize;
        float minZ = chunkIndex.y * chunkSize;
        float maxZ = (chunkIndex.y + 1) * chunkSize;
        
        Vector3 center = new Vector3(
            (minX + maxX) * 0.5f,
            0f, // 默认高度
            (minZ + maxZ) * 0.5f
        );
        Vector3 size = new Vector3(chunkSize, 100f, chunkSize); // 默认高度100
        
        return new Bounds(center, size);
    }

    private IEnumerator ProcessLoadQueue()
    {
        while (true)
        {
            int loadedThisFrame = 0;
            
            while (loadQueue.Count > 0 && loadedThisFrame < maxLoadPerFrame)
            {
                Vector2Int chunkIndex = loadQueue.Dequeue();
                
                if (!loadedChunks.ContainsKey(chunkIndex) && !loadingChunks.Contains(chunkIndex))
                {
                    LoadChunkAsync(chunkIndex);
                    loadedThisFrame++;
                }
            }
            
            yield return null;
        }
    }

    private IEnumerator ProcessUnloadQueue()
    {
        while (true)
        {
            int unloadedThisFrame = 0;
            
            while (unloadQueue.Count > 0 && unloadedThisFrame < maxUnloadPerFrame)
            {
                Vector2Int chunkIndex = unloadQueue.Dequeue();
                
                if (loadedChunks.ContainsKey(chunkIndex))
                {
                    UnloadChunkImmediate(chunkIndex);
                    unloadedThisFrame++;
                }
            }
            
            yield return null;
        }
    }

    private IEnumerator CheckDelayedUnload()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            List<Vector2Int> toUnload = new List<Vector2Int>();
            
            foreach (var kvp in chunksToUnload)
            {
                if (Time.time - kvp.Value >= unloadDelayTime)
                {
                    toUnload.Add(kvp.Key);
                }
            }
            
            foreach (var chunkIndex in toUnload)
            {
                chunksToUnload.Remove(chunkIndex);
                
                if (!unloadQueue.Contains(chunkIndex))
                {
                    unloadQueue.Enqueue(chunkIndex);
                }
            }
        }
    }

    private void LoadChunkAsync(Vector2Int chunkIndex)
    {
        StartCoroutine(LoadChunkCoroutine(chunkIndex));
    }

    private IEnumerator LoadChunkCoroutine(Vector2Int chunkIndex)
    {
        string sceneName = GetSceneName(chunkIndex);
        
        // 再次检查场景是否存在
        if (!IsChunkSceneValid(chunkIndex))
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[ChunkStreaming] 场景不存在,跳过加载: {sceneName}");
            }
            yield break;
        }
        
        loadingChunks.Add(chunkIndex);
        
        if (showDebugInfo)
        {
            Debug.Log($"[ChunkStreaming] 开始加载: {sceneName}");
        }

        var sceneHandle = EasyAsset.Instance.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        
        yield return sceneHandle.WaitForCompletion();
        
        loadingChunks.Remove(chunkIndex);
        
        if (sceneHandle.Status == EProviderStatus.Succeed)
        {
            loadedChunks[chunkIndex] = sceneHandle;
            
            if (showDebugInfo)
            {
                Debug.Log($"[ChunkStreaming] 加载完成: {sceneName}");
            }
        }
        else
        {
            // 加载失败,标记为无效
            invalidChunks.Add(chunkIndex);
            validChunks.Remove(chunkIndex);
            
            Debug.LogError($"[ChunkStreaming] 加载失败: {sceneName}, 错误: {sceneHandle.LastError}");
        }
    }

    private void UnloadChunkImmediate(Vector2Int chunkIndex)
    {
        if (loadedChunks.TryGetValue(chunkIndex, out SceneHandle sceneHandle))
        {
            string sceneName = GetSceneName(chunkIndex);
        
            if (showDebugInfo)
            {
                Debug.Log($"[ChunkStreaming] 卸载: {sceneName}");
            }

            // 直接使用EasyAsset卸载
            StartCoroutine(UnloadChunkCoroutine(sceneName, chunkIndex));
        }
    }

    private IEnumerator UnloadChunkCoroutine(string sceneName, Vector2Int chunkIndex)
    {
        yield return loadedChunks[chunkIndex].UnloadAsync();
        loadedChunks.Remove(chunkIndex);
    }

    
    private void OnDestroy()
    {
        if (!IsInitialized) return;
        
        StopAllCoroutines();
        
        foreach (var kvp in loadedChunks)
        {
            kvp.Value.UnloadAsync();
        }
        
        loadedChunks.Clear();
        loadingChunks.Clear();
        chunksToUnload.Clear();
        loadQueue.Clear();
        unloadQueue.Clear();
        validChunks.Clear();
        invalidChunks.Clear();
        chunksInFrustum.Clear();
        
        IsInitialized = false;
        
        Debug.Log($"[ChunkStreaming] 已清理所有Chunk");
    }

    private void OnDrawGizmos()
    {
        if (!enableGizmos || !Application.isPlaying || mainCamera == null) return;
        
        // 绘制当前chunk(黄色)
        Vector3 currentCenter = new Vector3(
            (currentChunkIndex.x + 0.5f) * chunkSize,
            mainCamera.transform.position.y,
            (currentChunkIndex.y + 0.5f) * chunkSize
        );
        Gizmos.color = Color.yellow;
        DrawChunkBounds(currentCenter, chunkSize);
        
        // 绘制检测范围(青色虚线)
        DrawRadiusGizmo(currentChunkIndex, maxCheckRadius, new Color(0, 1, 1, 0.3f));
        
        // 绘制卸载半径(红色虚线)
        DrawRadiusGizmo(currentChunkIndex, unloadRadius, new Color(1, 0, 0, 0.3f));
        
        // 绘制在视锥体内的chunk(蓝色边框)
        foreach (var chunkIndex in chunksInFrustum)
        {
            Vector3 center = GetChunkCenter(chunkIndex);
            Gizmos.color = new Color(0, 0.5f, 1, 0.8f);
            DrawChunkBounds(center, chunkSize);
        }
        
        // 绘制已加载的chunk(绿色)
        foreach (var chunkIndex in loadedChunks.Keys)
        {
            Vector3 center = GetChunkCenter(chunkIndex);
            Gizmos.color = new Color(0, 1, 0, 0.8f);
            DrawChunkBounds(center, chunkSize);
        }
        
        // 绘制正在加载的chunk(黄色)
        foreach (var chunkIndex in loadingChunks)
        {
            Vector3 center = GetChunkCenter(chunkIndex);
            Gizmos.color = Color.yellow;
            DrawChunkBounds(center, chunkSize);
        }
        
        // 绘制待卸载的chunk(橙色)
        foreach (var chunkIndex in chunksToUnload.Keys)
        {
            Vector3 center = GetChunkCenter(chunkIndex);
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            DrawChunkBounds(center, chunkSize);
        }
        
        // 绘制无效的chunk(灰色X)
        foreach (var chunkIndex in invalidChunks)
        {
            int distance = Mathf.Max(
                Mathf.Abs(chunkIndex.x - currentChunkIndex.x),
                Mathf.Abs(chunkIndex.y - currentChunkIndex.y)
            );
            
            Vector3 center = GetChunkCenter(chunkIndex);
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            DrawChunkX(center, chunkSize * 0.3f);
        }
    }

    private void DrawRadiusGizmo(Vector2Int center, int radius, Color color)
    {
        Gizmos.color = color;
        float worldRadius = (radius + 0.5f) * chunkSize;
        Vector3 worldCenter = new Vector3(
            (center.x + 0.5f) * chunkSize,
            mainCamera.transform.position.y,
            (center.y + 0.5f) * chunkSize
        );
        
        Vector3 p0 = worldCenter + new Vector3(-worldRadius, 0, -worldRadius);
        Vector3 p1 = worldCenter + new Vector3(worldRadius, 0, -worldRadius);
        Vector3 p2 = worldCenter + new Vector3(worldRadius, 0, worldRadius);
        Vector3 p3 = worldCenter + new Vector3(-worldRadius, 0, worldRadius);
        
        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }

    private Vector3 GetChunkCenter(Vector2Int chunkIndex)
    {
        return new Vector3(
            (chunkIndex.x + 0.5f) * chunkSize,
            mainCamera.transform.position.y,
            (chunkIndex.y + 0.5f) * chunkSize
        );
    }

    private void DrawChunkBounds(Vector3 center, float size)
    {
        float halfSize = size * 0.5f;
        Vector3 p0 = center + new Vector3(-halfSize, 0, -halfSize);
        Vector3 p1 = center + new Vector3(halfSize, 0, -halfSize);
        Vector3 p2 = center + new Vector3(halfSize, 0, halfSize);
        Vector3 p3 = center + new Vector3(-halfSize, 0, halfSize);
        
        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }

    private void DrawChunkX(Vector3 center, float size)
    {
        Gizmos.DrawLine(center + new Vector3(-size, 0, -size), center + new Vector3(size, 0, size));
        Gizmos.DrawLine(center + new Vector3(-size, 0, size), center + new Vector3(size, 0, -size));
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!showDebugInfo || !IsInitialized) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        
        string info = $"当前Chunk: {currentChunkIndex}\n" +
                     $"视锥体内: {chunksInFrustum.Count}个\n" +
                     $"已加载: {loadedChunks.Count}个\n" +
                     $"加载中: {loadingChunks.Count}个\n" +
                     $"加载队列: {loadQueue.Count}个\n" +
                     $"待卸载: {chunksToUnload.Count}个\n" +
                     $"卸载队列: {unloadQueue.Count}个\n" +
                     $"有效场景: {validChunks.Count}个\n" +
                     $"无效场景: {invalidChunks.Count}个\n" +
                     $"相机位置: ({mainCamera.transform.position.x:F1}, {mainCamera.transform.position.z:F1})";
        
        GUI.Label(new Rect(10, 10, 300, 230), info, style);
    }
#endif
}