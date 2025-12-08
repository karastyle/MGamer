using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 场景分块工具的通用工具类
/// </summary>
public static class ChunkToolUtility
{
    /// <summary>
    /// Unity 6 的所有Static标志   屏蔽BatchingStatic，为了配合lightmap手动管理
    /// </summary>
    public static StaticEditorFlags AllStaticFlags = 
        StaticEditorFlags.ContributeGI |
        StaticEditorFlags.OccluderStatic |
        StaticEditorFlags.OccludeeStatic |
        StaticEditorFlags.ReflectionProbeStatic;

    /// <summary>
    /// 收集渲染节点
    /// 修改：支持 LODGroup、MeshRenderer、ParticleSystem
    /// 只要包含其中一个组件，就不再递归子节点（将整个节点视为一个Chunk物体）
    /// </summary>
    public static void CollectMeshRenderers(Transform parent, List<GameObject> result)
    {
        // 检查是否是目标渲染节点
        // 优先级：LODGroup 通常在父节点，MeshRenderer/ParticleSystem 可能在同级
        if (parent.GetComponent<LODGroup>() != null || 
            parent.GetComponent<MeshRenderer>() != null || 
            parent.GetComponent<ParticleSystem>() != null)
        {
            result.Add(parent.gameObject);
            return; // ✅ 找到聚合节点后，不再递归子节点
        }

        // 未找到目标组件，继续递归查找子节点
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            CollectMeshRenderers(child, result);
        }
    }

    /// <summary>
    /// 递归设置Static标志
    /// </summary>
    public static void SetStaticRecursively(GameObject obj, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(obj, flags);

        foreach (Transform child in obj.transform)
        {
            SetStaticRecursively(child.gameObject, flags);
        }
    }

    /// <summary>
    /// 递归设置物体只贡献光照，不接收光照烘焙
    /// 用于相邻chunk参与烘焙但不被烘焙
    /// </summary>
    public static void SetContributeOnlyRecursively(GameObject obj)
    {
        // 设置为只影响其他物体的光照，自己不被烘焙
        GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            SerializedObject so = new SerializedObject(r);
            SerializedProperty receiveGIProp = so.FindProperty("m_ReceiveGI");
            if (receiveGIProp != null)
            {
                receiveGIProp.intValue = (int)ReceiveGI.LightProbes; // 不接收lightmap
                so.ApplyModifiedProperties();
            }
        }

        foreach (Transform child in obj.transform)
        {
            SetContributeOnlyRecursively(child.gameObject);
        }
    }

    /// <summary>
    /// 解析chunk场景名称，获取chunk索引
    /// </summary>
    /// <param name="sceneName">场景名称，如 "Chunk_0_1"</param>
    /// <param name="chunkIndex">输出的chunk索引</param>
    /// <returns>是否解析成功</returns>
    public static bool ParseChunkIndex(string sceneName, out Vector2Int chunkIndex)
    {
        chunkIndex = Vector2Int.zero;

        if (!sceneName.StartsWith("Chunk_"))
        {
            return false;
        }

        string[] parts = sceneName.Split('_');
        if (parts.Length != 3)
        {
            return false;
        }

        if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
        {
            chunkIndex = new Vector2Int(x, y);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取相邻的8个chunk索引
    /// </summary>
    public static Vector2Int[] GetNeighborChunkIndices(Vector2Int chunkIndex)
    {
        return new Vector2Int[]
        {
            new Vector2Int(chunkIndex.x - 1, chunkIndex.y),     // 左
            new Vector2Int(chunkIndex.x + 1, chunkIndex.y),     // 右
            new Vector2Int(chunkIndex.x, chunkIndex.y - 1),     // 下
            new Vector2Int(chunkIndex.x, chunkIndex.y + 1),     // 上
            new Vector2Int(chunkIndex.x - 1, chunkIndex.y - 1), // 左下
            new Vector2Int(chunkIndex.x + 1, chunkIndex.y - 1), // 右下
            new Vector2Int(chunkIndex.x - 1, chunkIndex.y + 1), // 左上
            new Vector2Int(chunkIndex.x + 1, chunkIndex.y + 1), // 右上
        };
    }

    /// <summary>
    /// 计算物体所属的chunk索引
    /// </summary>
    public static Vector2Int CalculateChunkIndex(Vector3 position, float chunkSize)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }

    /// <summary>
    /// 按chunk分组物体
    /// </summary>
    public static Dictionary<Vector2Int, List<GameObject>> GroupObjectsByChunk(
        List<GameObject> objects, 
        float chunkSize)
    {
        Dictionary<Vector2Int, List<GameObject>> chunks = new Dictionary<Vector2Int, List<GameObject>>();

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;

            Vector3 pos = obj.transform.position;
            Vector2Int chunkIndex = CalculateChunkIndex(pos, chunkSize);

            if (!chunks.ContainsKey(chunkIndex))
            {
                chunks[chunkIndex] = new List<GameObject>();
            }

            chunks[chunkIndex].Add(obj);
        }

        return chunks;
    }
}