using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HPA* 分层网格
/// </summary>
public class HierarchicalGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridWorldSize = new Vector2(100, 100);
    public float fineNodeRadius = 0.5f;
    
    [Header("HPA Settings")]
    public int clusterSize = 8;  // 每个簇的大小（8x8 细格子）
    public HPAData hpaData;  // 预处理数据引用
    
    [Header("Manual Obstacles")]
    [HideInInspector] public List<Vector2Int> manualObstacles = new List<Vector2Int>();
    
    [Header("Visualization")]
    public bool displayFineGrid = false;
    public bool displayClusters = true;
    public bool displayEntrancePoints = true;
    public bool displayAbstractPath = true;
    
    // 细网格
    private AStarNode[,] fineGrid;
    private int fineGridSizeX, fineGridSizeY;
    private float fineNodeDiameter;
    
    // HPA 运行时数据
    private Dictionary<int, Cluster> clusterDict;
    private Dictionary<int, EntrancePoint> entrancePointDict;
    private Dictionary<int, List<AbstractEdge>> adjacencyList;
    
    // 调试可视化
    [HideInInspector] public List<AStarNode> finePath;
    [HideInInspector] public List<EntrancePoint> abstractPath;
    
    public int MaxFineSize => fineGridSizeX * fineGridSizeY;
    
    void Awake()
    {
        InitializeGrid();
        LoadHPAData();
    }
    
    public void InitializeGrid()
    {
        fineNodeDiameter = fineNodeRadius * 2;
        fineGridSizeX = Mathf.RoundToInt(gridWorldSize.x / fineNodeDiameter);
        fineGridSizeY = Mathf.RoundToInt(gridWorldSize.y / fineNodeDiameter);
        CreateFineGrid();
    }
    
    void CreateFineGrid()
    {
        fineGrid = new AStarNode[fineGridSizeX, fineGridSizeY];
        Vector3 worldBottomLeft = transform.position 
            - Vector3.right * gridWorldSize.x / 2 
            - Vector3.forward * gridWorldSize.y / 2;
        
        for (int x = 0; x < fineGridSizeX; x++)
        {
            for (int y = 0; y < fineGridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft 
                    + Vector3.right * (x * fineNodeDiameter + fineNodeRadius) 
                    + Vector3.forward * (y * fineNodeDiameter + fineNodeRadius);
                
                bool walkable = !IsManualObstacle(x, y);
                fineGrid[x, y] = new AStarNode(walkable, worldPoint, x, y);
            }
        }
    }
    
    /// <summary>
    /// 加载 HPA 预处理数据
    /// </summary>
    public void LoadHPAData()
    {
        if (hpaData == null)
        {
            Debug.LogWarning("HPA Data not found! Please preprocess first.");
            return;
        }
        
        // 验证数据
        if (hpaData.fineGridSizeX != fineGridSizeX || hpaData.fineGridSizeY != fineGridSizeY)
        {
            Debug.LogError("HPA Data grid size mismatch! Please reprocess.");
            return;
        }
        
        // 构建运行时数据结构
        clusterDict = new Dictionary<int, Cluster>();
        entrancePointDict = new Dictionary<int, EntrancePoint>();
        adjacencyList = new Dictionary<int, List<AbstractEdge>>();
        
        // 加载簇
        foreach (var clusterData in hpaData.clusters)
        {
            var cluster = new Cluster(clusterData);
            clusterDict[cluster.id] = cluster;
        }
        
        // 加载入口点
        foreach (var epData in hpaData.entrancePoints)
        {
            var ep = new EntrancePoint(epData);
            entrancePointDict[ep.id] = ep;
        }
        
        // 加载抽象图边
        foreach (var edgeData in hpaData.abstractEdges)
        {
            if (!adjacencyList.ContainsKey(edgeData.fromEntranceId))
            {
                adjacencyList[edgeData.fromEntranceId] = new List<AbstractEdge>();
            }
            
            adjacencyList[edgeData.fromEntranceId].Add(new AbstractEdge
            {
                fromId = edgeData.fromEntranceId,
                toId = edgeData.toEntranceId,
                cost = edgeData.cost,
                isInter = edgeData.isInter
            });
            
            // 双向边
            if (!adjacencyList.ContainsKey(edgeData.toEntranceId))
            {
                adjacencyList[edgeData.toEntranceId] = new List<AbstractEdge>();
            }
            
            adjacencyList[edgeData.toEntranceId].Add(new AbstractEdge
            {
                fromId = edgeData.toEntranceId,
                toId = edgeData.fromEntranceId,
                cost = edgeData.cost,
                isInter = edgeData.isInter
            });
        }
        
        Debug.Log($"HPA Data loaded: {clusterDict.Count} clusters, {entrancePointDict.Count} entrance points, {hpaData.abstractEdges.Count} edges");
    }
    
    // 获取世界坐标对应的细网格节点
    public AStarNode FineNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        
        int x = Mathf.RoundToInt((fineGridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((fineGridSizeY - 1) * percentY);
        
        if (fineGrid == null || x < 0 || x >= fineGridSizeX || y < 0 || y >= fineGridSizeY)
        {
            return null;
        }
        
        return fineGrid[x, y];
    }
    
    public AStarNode GetFineNode(int x, int y)
    {
        if (x < 0 || x >= fineGridSizeX || y < 0 || y >= fineGridSizeY)
            return null;
        return fineGrid[x, y];
    }
    
    // 获取细网格邻居
    public List<AStarNode> GetFineNeighbours(AStarNode node)
    {
        List<AStarNode> neighbours = new List<AStarNode>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                
                if (checkX >= 0 && checkX < fineGridSizeX && checkY >= 0 && checkY < fineGridSizeY)
                {
                    AStarNode neighbour = fineGrid[checkX, checkY];
                    
                    if (x != 0 && y != 0)
                    {
                        AStarNode side1 = fineGrid[node.gridX + x, node.gridY];
                        AStarNode side2 = fineGrid[node.gridX, node.gridY + y];
                        
                        if (side1.walkable && side2.walkable)
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                    else
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }
        }
        
        return neighbours;
    }
    
    // 获取点所在的簇
    public Cluster GetClusterAtPoint(Vector3 worldPos)
    {
        AStarNode node = FineNodeFromWorldPoint(worldPos);
        if (node == null) return null;
        
        int clusterX = node.gridX / clusterSize;
        int clusterY = node.gridY / clusterSize;
        
        foreach (var cluster in clusterDict.Values)
        {
            if (cluster.gridX == clusterX && cluster.gridY == clusterY)
            {
                return cluster;
            }
        }
        
        return null;
    }
    
    // 获取抽象图邻居
    public List<AbstractEdge> GetAbstractNeighbours(int entranceId)
    {
        if (adjacencyList.ContainsKey(entranceId))
        {
            return adjacencyList[entranceId];
        }
        return new List<AbstractEdge>();
    }
    
    public EntrancePoint GetEntrancePoint(int id)
    {
        return entrancePointDict.ContainsKey(id) ? entrancePointDict[id] : null;
    }
    
    public void SetObstacle(int x, int y, bool isObstacle)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (isObstacle)
        {
            if (!manualObstacles.Contains(pos))
            {
                manualObstacles.Add(pos);
            }
        }
        else
        {
            manualObstacles.Remove(pos);
        }
    }
    
    public bool IsManualObstacle(int x, int y)
    {
        return manualObstacles.Contains(new Vector2Int(x, y));
    }
    
    // 可视化
    public void DrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 绘制簇
        if (displayClusters && clusterDict != null)
        {
            foreach (var cluster in clusterDict.Values)
            {
                Gizmos.color = cluster.walkable ? new Color(0, 0, 1, 0.1f) : new Color(1, 0, 0, 0.2f);
                Vector3 size = new Vector3(clusterSize * fineNodeDiameter, 0.1f, clusterSize * fineNodeDiameter);
                Gizmos.DrawCube(cluster.worldCenter, size);
                
                Gizmos.color = Color.blue * 0.5f;
                Gizmos.DrawWireCube(cluster.worldCenter, size);
            }
        }
        
        // 绘制细网格
        if (displayFineGrid && fineGrid != null)
        {
            foreach (AStarNode node in fineGrid)
            {
                if (finePath != null && finePath.Contains(node))
                {
                    Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (fineNodeDiameter - 0.1f));
                }
                else
                {
                    Gizmos.color = node.walkable ? new Color(1, 1, 1, 0.02f) : new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (fineNodeDiameter - 0.1f));
                }
            }
        }
        
        // 绘制入口点
        if (displayEntrancePoints && entrancePointDict != null)
        {
            foreach (var ep in entrancePointDict.Values)
            {
                Gizmos.color = ep.isInter ? Color.yellow : Color.green;
                Gizmos.DrawSphere(ep.worldPosition + Vector3.up * 0.3f, 0.3f);
                
                // 绘制连接线
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                if (ep.cluster2Id != -1)
                {
                    var cluster1 = clusterDict.ContainsKey(ep.cluster1Id) ? clusterDict[ep.cluster1Id] : null;
                    var cluster2 = clusterDict.ContainsKey(ep.cluster2Id) ? clusterDict[ep.cluster2Id] : null;
                    
                    if (cluster1 != null && cluster2 != null)
                    {
                        Gizmos.DrawLine(
                            ep.worldPosition + Vector3.up * 0.3f,
                            cluster1.worldCenter + Vector3.up * 0.3f
                        );
                        Gizmos.DrawLine(
                            ep.worldPosition + Vector3.up * 0.3f,
                            cluster2.worldCenter + Vector3.up * 0.3f
                        );
                    }
                }
            }
        }
        
        // 绘制抽象路径
        if (displayAbstractPath && abstractPath != null && abstractPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < abstractPath.Count - 1; i++)
            {
                Gizmos.DrawLine(
                    abstractPath[i].worldPosition + Vector3.up * 0.5f,
                    abstractPath[i + 1].worldPosition + Vector3.up * 0.5f
                );
            }
            
            foreach (var ep in abstractPath)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(ep.worldPosition + Vector3.up * 0.5f, 0.4f);
            }
        }
    }
}

/// <summary>
/// 运行时簇
/// </summary>
public class Cluster
{
    public int id;
    public int gridX, gridY;
    public Vector3 worldCenter;
    public int startX, startY, endX, endY;
    public bool walkable;
    public List<int> entrancePointIds;
    
    public Cluster(ClusterData data)
    {
        this.id = data.id;
        this.gridX = data.gridX;
        this.gridY = data.gridY;
        this.worldCenter = data.worldCenter;
        this.startX = data.startX;
        this.startY = data.startY;
        this.endX = data.endX;
        this.endY = data.endY;
        this.walkable = data.walkable;
        this.entrancePointIds = new List<int>(data.entrancePointIds);
    }
}

/// <summary>
/// 运行时入口点
/// </summary>
public class EntrancePoint
{
    public int id;
    public Vector2Int fineGridPos;
    public Vector3 worldPosition;
    public int cluster1Id;
    public int cluster2Id;
    public bool isInter;
    
    public EntrancePoint(EntrancePointData data)
    {
        this.id = data.id;
        this.fineGridPos = data.fineGridPos;
        this.worldPosition = data.worldPosition;
        this.cluster1Id = data.cluster1Id;
        this.cluster2Id = data.cluster2Id;
        this.isInter = data.isInter;
    }
}

/// <summary>
/// 抽象图边
/// </summary>
public struct AbstractEdge
{
    public int fromId;
    public int toId;
    public float cost;
    public bool isInter;
}