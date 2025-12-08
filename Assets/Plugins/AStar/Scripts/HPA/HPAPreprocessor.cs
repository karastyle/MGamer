using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HPA* 预处理器
/// </summary>
public class HPAPreprocessor
{
    private HierarchicalGrid grid;
    private HPAData hpaData;
    
    private int nextEntranceId = 0;
    
    public HPAPreprocessor(HierarchicalGrid grid)
    {
        this.grid = grid;
    }
    
    /// <summary>
    /// 执行完整预处理
    /// </summary>
    public HPAData Preprocess()
    {
        Debug.Log("Starting HPA* preprocessing...");
        
        hpaData = ScriptableObject.CreateInstance<HPAData>();
        hpaData.clusterSize = grid.clusterSize;
        hpaData.gridWorldSize = grid.gridWorldSize;
        hpaData.fineNodeRadius = grid.fineNodeRadius;
        hpaData.fineGridSizeX = Mathf.RoundToInt(grid.gridWorldSize.x / (grid.fineNodeRadius * 2));
        hpaData.fineGridSizeY = Mathf.RoundToInt(grid.gridWorldSize.y / (grid.fineNodeRadius * 2));
        
        // 步骤 1: 划分簇
        Debug.Log("Step 1: Creating clusters...");
        CreateClusters();
        
        // 步骤 2: 识别入口点
        Debug.Log("Step 2: Finding entrance points...");
        FindEntrancePoints();
        
        // 步骤 3: 构建抽象图
        Debug.Log("Step 3: Building abstract graph...");
        BuildAbstractGraph();
        
        Debug.Log($"Preprocessing complete: {hpaData.clusters.Count} clusters, {hpaData.entrancePoints.Count} entrance points, {hpaData.abstractEdges.Count} edges");

        DebugPrintAllEntrancePoints();
        
        return hpaData;
    }
    
    void DebugPrintAllEntrancePoints()
    {
        Debug.Log("=== All Entrance Points (with positions) ===");
    
        foreach (var ep in hpaData.entrancePoints)
        {
            Debug.Log($"EP {ep.id}: grid=({ep.fineGridPos.x}, {ep.fineGridPos.y}), " +
                $"C{ep.cluster1Id}↔C{ep.cluster2Id}");
        }
    
        Debug.Log("\n=== Sample Cluster Analysis ===");
    
        // 🔑 分析第一个内部簇 Cluster 14 (1,1)，应该有8个入口点
        var cluster14 = hpaData.clusters.Find(c => c.id == 14);
        if (cluster14 != null)
        {
            Debug.Log($"\n--- Cluster 14 (1,1) Analysis ---");
            Debug.Log($"Grid bounds: x=[{cluster14.startX}, {cluster14.endX}), y=[{cluster14.startY}, {cluster14.endY})");
            Debug.Log($"Expected: Right boundary x={cluster14.endX}, Top boundary y={cluster14.endY}");
            Debug.Log($"Entrance Points:");
        
            foreach (int epId in cluster14.entrancePointIds)
            {
                var ep = hpaData.entrancePoints.Find(e => e.id == epId);
                if (ep != null)
                {
                    string direction = "";
                
                    // 判断入口点在哪条边
                    if (ep.fineGridPos.x == cluster14.startX)
                        direction = "LEFT";
                    else if (ep.fineGridPos.x == cluster14.endX)
                        direction = "RIGHT";
                    else if (ep.fineGridPos.y == cluster14.startY)
                        direction = "BOTTOM";
                    else if (ep.fineGridPos.y == cluster14.endY)
                        direction = "TOP";
                
                    Debug.Log($"  EP{ep.id} at ({ep.fineGridPos.x}, {ep.fineGridPos.y}) - {direction} edge");
                }
            }
        }
    }
    
    /// <summary>
    /// 步骤 1: 划分簇
    /// </summary>
    void CreateClusters()
    {
        int clusterCols = Mathf.CeilToInt(hpaData.fineGridSizeX / (float)hpaData.clusterSize);
        int clusterRows = Mathf.CeilToInt(hpaData.fineGridSizeY / (float)hpaData.clusterSize);
        
        int clusterId = 0;
        
        for (int cy = 0; cy < clusterRows; cy++)
        {
            for (int cx = 0; cx < clusterCols; cx++)
            {
                int startX = cx * hpaData.clusterSize;
                int startY = cy * hpaData.clusterSize;
                int endX = Mathf.Min(startX + hpaData.clusterSize, hpaData.fineGridSizeX);
                int endY = Mathf.Min(startY + hpaData.clusterSize, hpaData.fineGridSizeY);
                
                // 计算簇中心
                float centerX = (startX + endX - 1) * 0.5f;
                float centerY = (startY + endY - 1) * 0.5f;
                
                Vector3 worldBottomLeft = grid.transform.position 
                    - Vector3.right * grid.gridWorldSize.x / 2 
                    - Vector3.forward * grid.gridWorldSize.y / 2;
                
                float nodeDiameter = grid.fineNodeRadius * 2;
                Vector3 worldCenter = worldBottomLeft 
                    + Vector3.right * (centerX * nodeDiameter + grid.fineNodeRadius) 
                    + Vector3.forward * (centerY * nodeDiameter + grid.fineNodeRadius);
                
                // 检查是否可行走
                bool walkable = CheckClusterWalkable(startX, startY, endX, endY);
                
                ClusterData cluster = new ClusterData
                {
                    id = clusterId++,
                    gridX = cx,
                    gridY = cy,
                    worldCenter = worldCenter,
                    startX = startX,
                    startY = startY,
                    endX = endX,
                    endY = endY,
                    walkable = walkable,
                    entrancePointIds = new List<int>()
                };
                
                hpaData.clusters.Add(cluster);
            }
        }
    }
    
    bool CheckClusterWalkable(int startX, int startY, int endX, int endY)
    {
        int walkableCount = 0;
        int totalCount = 0;
        
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                totalCount++;
                AStarNode node = grid.GetFineNode(x, y);
                if (node != null && node.walkable)
                {
                    walkableCount++;
                }
            }
        }
        
        return walkableCount >= totalCount * 0.3f;  // 30% 可行走即可
    }
    
    /// <summary>
    /// 步骤 2: 识别入口点
    /// </summary>
    void FindEntrancePoints()
    {
        // 对每对相邻簇找入口点
        for (int i = 0; i < hpaData.clusters.Count; i++)
        {
            ClusterData c1 = hpaData.clusters[i];
            
            // 检查右邻居
            ClusterData c2Right = FindCluster(c1.gridX + 1, c1.gridY);
            if (c2Right != null)
            {
                FindEntrancesHorizontal(c1, c2Right);
            }
            
            // 检查上邻居
            ClusterData c2Up = FindCluster(c1.gridX, c1.gridY + 1);
            if (c2Up != null)
            {
                FindEntrancesVertical(c1, c2Up);
            }
        }
    }
    
    ClusterData FindCluster(int gridX, int gridY)
    {
        foreach (var cluster in hpaData.clusters)
        {
            if (cluster.gridX == gridX && cluster.gridY == gridY)
            {
                return cluster;
            }
        }
        return null;
    }
    
    void FindEntrancesHorizontal(ClusterData c1, ClusterData c2)
    {
        // 使用 C2 的左边界（确保入口点在正确位置）
        int boundaryX = c2.startX;
        int startY = Mathf.Max(c1.startY, c2.startY);
        int endY = Mathf.Min(c1.endY, c2.endY);
    
        List<Vector2Int> walkableStretch = new List<Vector2Int>();
    
        for (int y = startY; y < endY; y++)
        {
            AStarNode node = grid.GetFineNode(boundaryX, y);
        
            if (node != null && node.walkable)
            {
                walkableStretch.Add(new Vector2Int(boundaryX, y));
            }
            else
            {
                if (walkableStretch.Count > 0)
                {
                    CreateEntrancePair(c1, c2, walkableStretch);
                    walkableStretch.Clear();
                }
            }
        }
    
        if (walkableStretch.Count > 0)
        {
            CreateEntrancePair(c1, c2, walkableStretch);
        }
    
        Debug.Log($"Horizontal C{c1.id}→C{c2.id}: boundaryX={boundaryX}, Y=[{startY}, {endY}), stretch={walkableStretch.Count}");
    }

    void FindEntrancesVertical(ClusterData c1, ClusterData c2)
    {
        // 使用 C2 的下边界
        int boundaryY = c2.startY;
        int startX = Mathf.Max(c1.startX, c2.startX);
        int endX = Mathf.Min(c1.endX, c2.endX);
    
        List<Vector2Int> walkableStretch = new List<Vector2Int>();
    
        for (int x = startX; x < endX; x++)
        {
            AStarNode node = grid.GetFineNode(x, boundaryY);
        
            if (node != null && node.walkable)
            {
                walkableStretch.Add(new Vector2Int(x, boundaryY));
            }
            else
            {
                if (walkableStretch.Count > 0)
                {
                    CreateEntrancePair(c1, c2, walkableStretch);
                    walkableStretch.Clear();
                }
            }
        }
    
        if (walkableStretch.Count > 0)
        {
            CreateEntrancePair(c1, c2, walkableStretch);
        }
    
        Debug.Log($"Vertical C{c1.id}→C{c2.id}: boundaryY={boundaryY}, X=[{startX}, {endX}), stretch={walkableStretch.Count}");
    }
    
    void CreateEntrancePair(ClusterData c1, ClusterData c2, List<Vector2Int> stretch)
    {
        if (stretch.Count == 0) return;
        
        // HPA*：只保留首尾两个入口点
        CreateEntrancePoint(c1, c2, stretch[0]);
        
        if (stretch.Count > 1)
        {
            CreateEntrancePoint(c1, c2, stretch[stretch.Count - 1]);
        }
    }
    
    void CreateEntrancePoint(ClusterData c1, ClusterData c2, Vector2Int gridPos)
    {
        AStarNode node = grid.GetFineNode(gridPos.x, gridPos.y);
        if (node == null) return;
        
        EntrancePointData ep = new EntrancePointData
        {
            id = nextEntranceId++,
            fineGridPos = gridPos,
            worldPosition = node.worldPosition,
            cluster1Id = c1.id,
            cluster2Id = c2.id,
            isInter = true
        };
        
        hpaData.entrancePoints.Add(ep);
        c1.entrancePointIds.Add(ep.id);
        c2.entrancePointIds.Add(ep.id);
    }
    
    /// <summary>
    /// 步骤 3: 构建抽象图（修正版）
    /// </summary>
    void BuildAbstractGraph()
    {
        // 用于去重的哈希集
        HashSet<(int, int)> processedPairs = new HashSet<(int, int)>();
    
        // 对每个簇，计算簇内入口点之间的距离
        foreach (var cluster in hpaData.clusters)
        {
            if (!cluster.walkable) continue;
        
            var entrances = GetClusterEntrances(cluster);
        
            for (int i = 0; i < entrances.Count; i++)
            {
                for (int j = i + 1; j < entrances.Count; j++)
                {
                    int id1 = entrances[i].id;
                    int id2 = entrances[j].id;
                
                    // 🔑 创建规范化的配对 ID（小的在前）
                    var pair = (Mathf.Min(id1, id2), Mathf.Max(id1, id2));
                
                    // 检查是否已经处理过这对入口点
                    if (processedPairs.Contains(pair))
                    {
                        continue;
                    }
                
                    float distance = ComputeIntraClusterDistance(entrances[i], entrances[j]);
                
                    if (distance > 0)
                    {
                        hpaData.abstractEdges.Add(new AbstractEdgeData
                        {
                            fromEntranceId = id1,
                            toEntranceId = id2,
                            cost = distance,
                            isInter = false
                        });
                    
                        // 标记为已处理
                        processedPairs.Add(pair);
                    }
                }
            }
        }
    
        Debug.Log($"Abstract graph built: {hpaData.abstractEdges.Count} unique edges");
    }
    
    List<EntrancePointData> GetClusterEntrances(ClusterData cluster)
    {
        List<EntrancePointData> result = new List<EntrancePointData>();
        
        foreach (int epId in cluster.entrancePointIds)
        {
            var ep = hpaData.entrancePoints.Find(e => e.id == epId);
            if (ep != null)
            {
                result.Add(ep);
            }
        }
        
        return result;
    }
    
    float ComputeIntraClusterDistance(EntrancePointData ep1, EntrancePointData ep2)
    {
        AStarNode node1 = grid.GetFineNode(ep1.fineGridPos.x, ep1.fineGridPos.y);
        AStarNode node2 = grid.GetFineNode(ep2.fineGridPos.x, ep2.fineGridPos.y);
        
        if (node1 == null || node2 == null) return -1;
        
        // 使用 A* 计算实际距离
        var path = FindFinePathForPreprocess(node1, node2);
        
        if (path == null) return -1;
        
        float distance = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            distance += Vector3.Distance(path[i].worldPosition, path[i + 1].worldPosition);
        }
        
        return distance;
    }
    
    List<AStarNode> FindFinePathForPreprocess(AStarNode start, AStarNode target)
    {
        MinHeap<AStarNode> openSet = new MinHeap<AStarNode>(grid.MaxFineSize);
        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();
        
        openSet.Add(start);
        
        while (openSet.Count > 0)
        {
            AStarNode current = openSet.RemoveFirst();
            closedSet.Add(current);
            
            if (current == target)
            {
                return RetracePath(start, target);
            }
            
            foreach (AStarNode neighbour in grid.GetFineNeighbours(current))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;
                
                int newCost = current.gCost + GetDistance(current, neighbour);
                
                if (newCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCost;
                    neighbour.hCost = GetDistance(neighbour, target);
                    neighbour.parent = current;
                    
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }
        
        return null;
    }
    
    List<AStarNode> RetracePath(AStarNode start, AStarNode end)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode current = end;
        
        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        
        path.Add(start);
        path.Reverse();
        return path;
    }
    
    int GetDistance(AStarNode a, AStarNode b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}