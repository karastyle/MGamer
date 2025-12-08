// 放置在 Assets/Scripts/JPSGrid.cs
using System.Collections.Generic;
using UnityEngine;

namespace JPSPlus
{
// 在编辑器模式下也运行，以便Scene视图交互
[ExecuteInEditMode]
public class JPSGrid : MonoBehaviour, ISerializationCallbackReceiver // <-- 添加接口
{
    [Header("Grid Configuration")]
    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Baking")]
    [Tooltip("烘焙数据保存的路径 (例如 'Assets/BakedData/MyGrid.asset')")]
    public string bakedDataSavePath = "Assets/JPSBakedData.asset";
    public JPSBakedData bakedData;
    
    // --- 序列化修正 ---
    // 这个List用于Unity保存
    [HideInInspector]
    public List<Int2> serializedObstacles = new List<Int2>();
    
    // 这个HashSet用于编辑器和烘焙时的快速查找
    // [System.NonSerialized] 确保它不会被Unity序列化
    [System.NonSerialized]
    public HashSet<Int2> manualObstacles = new HashSet<Int2>();
    // --- 修正结束 ---

    [Header("Gizmos")]
    public bool displayGridGizmos = true;
    public bool displayObstacleGizmos = true;

    // ======================================================
    //   !!!! 新增的预览颜色配置 !!!!
    // ======================================================
    [Header("Preview Colors")]
    [Tooltip("距离 > 0 (有效跳点)")]
    public Color previewColor_JumpPoint = Color.green;
    [Tooltip("距离 < 0 (该方向是墙)")]
    public Color previewColor_Wall = new Color(1f, 0.5f, 0.5f, 0.5f); // 浅红
    [Tooltip("距离 == 0 (边界或直线上的墙)")]
    public Color previewColor_Zero = new Color(0.5f, 0.5f, 0.5f, 0.4f); // 灰色
    // ======================================================

    [HideInInspector] public int GridSizeX;
    [HideInInspector] public int GridSizeY;
    [HideInInspector] public float NodeDiameter;
    [HideInInspector] public Vector3 GridWorldOrigin;

    void Awake()
    {
        InitializeGridParameters();
    }
    
    /// <summary>
    /// 初始化网格参数（不创建节点数组）
    /// </summary>
    public void InitializeGridParameters()
    {
        NodeDiameter = nodeRadius * 2;
        GridSizeX = Mathf.RoundToInt(gridWorldSize.x / NodeDiameter);
        GridSizeY = Mathf.RoundToInt(gridWorldSize.y / NodeDiameter);
        GridWorldOrigin = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
    }
    
    /// <summary>
    /// 根据世界坐标获取网格坐标
    /// </summary>
    public Int2 GetGridCoords(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - GridWorldOrigin;
        int x = Mathf.FloorToInt(localPos.x / NodeDiameter);
        int y = Mathf.FloorToInt(localPos.z / NodeDiameter);
        return new Int2(x, y);
    }
    
    /// <summary>
    /// 获取网格中心的世界坐标
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        return GridWorldOrigin + new Vector3(x * NodeDiameter + nodeRadius, 0, y * NodeDiameter + nodeRadius);
    }
    
    /// <summary>
    /// 检查一个格子是否是障碍物（用于烘焙）
    /// </summary>
    public bool IsObstacle(int x, int y)
    {
        // (现在使用 HashSet 进行快速查找)
        if (manualObstacles.Contains(new Int2(x, y)))
        {
            return true;
        }
        
        Vector3 worldPoint = GetWorldPosition(x, y);
        return Physics.CheckSphere(worldPoint, nodeRadius, obstacleLayer);
    }

    void OnDrawGizmos()
    {
        if (!displayGridGizmos) return;

        // 确保参数在编辑器中是最新的
        // (在OnDrawGizmos中频繁调用可能不好，但对于ExecuteInEditMode是必要的)
        InitializeGridParameters();

        // 绘制网格线
        Gizmos.color = new Color(1, 1, 1, 0.1f);
        for (int x = 0; x <= GridSizeX; x++)
        {
            Vector3 start = GetWorldPosition(x, 0) - new Vector3(nodeRadius, 0, nodeRadius);
            Vector3 end = GetWorldPosition(x, GridSizeY - 1) - new Vector3(nodeRadius, 0, -nodeRadius);
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= GridSizeY; y++)
        {
            Vector3 start = GetWorldPosition(0, y) - new Vector3(nodeRadius, 0, nodeRadius);
            Vector3 end = GetWorldPosition(GridSizeX - 1, y) - new Vector3(-nodeRadius, 0, nodeRadius);
            Gizmos.DrawLine(start, end);
        }

        // ======================================================
        //   !!!! 修正的代码块 !!!!
        // ======================================================
        // 绘制障碍物
        if (displayObstacleGizmos)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            // (从HashSet绘制)
            foreach (var obstacle in manualObstacles)
            {
                // 确保坐标在网格内
                if (obstacle.x >= 0 && obstacle.x < GridSizeX && obstacle.y >= 0 && obstacle.y < GridSizeY)
                {
                    Gizmos.DrawCube(GetWorldPosition(obstacle.x, obstacle.y), 
                        new Vector3(NodeDiameter - 0.05f, 0.1f, NodeDiameter - 0.05f));
                }
            }
        }
        // ======================================================
    }
    
    // --- 序列化回调 ---
    /// <summary>
    /// 在序列化（保存）之前调用
    /// </summary>
    public void OnBeforeSerialize()
    {
        // 将HashSet中的数据复制到List中，以便Unity保存
        serializedObstacles.Clear();
        foreach (Int2 obstacle in manualObstacles)
        {
            serializedObstacles.Add(obstacle);
        }
    }

    /// <summary>
    /// 在反序列化（加载）之后调用
    /// </summary>
    public void OnAfterDeserialize()
    {
        // 从List中恢复数据到HashSet
        manualObstacles.Clear();
        foreach (Int2 obstacle in serializedObstacles)
        {
            manualObstacles.Add(obstacle);
        }
    }
}
}