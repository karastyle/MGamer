using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HPA* 预处理数据（可序列化）
/// </summary>
[System.Serializable]
public class HPAData : ScriptableObject
{
    public int clusterSize = 8;
    public List<ClusterData> clusters = new List<ClusterData>();
    public List<EntrancePointData> entrancePoints = new List<EntrancePointData>();
    public List<AbstractEdgeData> abstractEdges = new List<AbstractEdgeData>();
    
    // 网格尺寸（用于验证）
    public Vector2 gridWorldSize;
    public float fineNodeRadius;
    public int fineGridSizeX;
    public int fineGridSizeY;
}

/// <summary>
/// 簇数据
/// </summary>
[System.Serializable]
public class ClusterData
{
    public int id;
    public int gridX, gridY;
    public Vector3 worldCenter;
    public int startX, startY;  // 细网格范围
    public int endX, endY;
    public bool walkable;  // 是否至少有部分可行走
    public List<int> entrancePointIds = new List<int>();  // 该簇的入口点 ID
}

/// <summary>
/// 入口点数据
/// </summary>
[System.Serializable]
public class EntrancePointData
{
    public int id;
    public Vector2Int fineGridPos;  // 细网格坐标
    public Vector3 worldPosition;
    public int cluster1Id;  // 所属簇1
    public int cluster2Id;  // 所属簇2（相邻簇，-1 表示边界入口点）
    public bool isInter;    // 是否跨簇入口点
}

/// <summary>
/// 抽象图边数据
/// </summary>
[System.Serializable]
public class AbstractEdgeData
{
    public int fromEntranceId;
    public int toEntranceId;
    public float cost;  // 预计算的距离
    public bool isInter;  // 是否跨簇边
}