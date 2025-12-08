using UnityEngine;
using System.Collections.Generic;

public class HierarchicalPathfinding : MonoBehaviour
{
    private HierarchicalGrid grid;
    
    [Header("Path Smoothing")]
    public bool enablePathSmoothing = true;
    public PathSmoothType smoothType = PathSmoothType.Raycast;
    public int catmullRomPointsPerSegment = 5;
    
    [Header("Corner Optimization")]
    public bool enableCornerOptimization = true;
    [Range(0.1f, 2f)] public float cornerOffset = 0.8f;
    [Range(10f, 90f)] public float minCornerAngle = 30f;
    
    public enum PathSmoothType
    {
        None,
        Raycast,
        CatmullRom,
        Simplified
    }
    
    // 临时入口点 ID（用于插入起点和终点）
    private int tempStartId = -1000;
    private int tempTargetId = -2000;
    
    void Awake()
    {
        grid = GetComponent<HierarchicalGrid>();
    }
    
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (grid.hpaData == null)
        {
            Debug.LogError("HPA Data not loaded! Cannot find path.");
            return null;
        }
        
        return FindHPAPath(startPos, targetPos);
    }
    
    /// <summary>
    /// HPA* 寻路主流程
    /// </summary>
    List<Vector3> FindHPAPath(Vector3 startPos, Vector3 targetPos)
    {
        // 步骤 1: 插入起点和终点到抽象图
        var startEntrance = InsertTemporaryNode(startPos, tempStartId);
        var targetEntrance = InsertTemporaryNode(targetPos, tempTargetId);
        
        if (startEntrance == null || targetEntrance == null)
        {
            Debug.LogError("Cannot insert start or target node");
            return null;
        }
        
        // 步骤 2: 在抽象图上运行 A*
        List<EntrancePoint> abstractPath = FindAbstractPath(startEntrance, targetEntrance);
        
        if (abstractPath == null || abstractPath.Count == 0)
        {
            Debug.LogWarning("No abstract path found");
            RemoveTemporaryNodes();
            return null;
        }
        
        grid.abstractPath = abstractPath;
        
        // 步骤 3: 细化路径
        List<AStarNode> finePath = RefinePath(abstractPath);
        
        // 步骤 4: 移除临时节点
        RemoveTemporaryNodes();
        
        if (finePath == null || finePath.Count == 0)
        {
            return null;
        }
        
        grid.finePath = finePath;
        
        // 步骤 5: 平滑和优化
        return ProcessPath(finePath);
    }
    
    /// <summary>
    /// 插入临时节点到抽象图
    /// </summary>
    EntrancePoint InsertTemporaryNode(Vector3 worldPos, int tempId)
    {
        AStarNode fineNode = grid.FineNodeFromWorldPoint(worldPos);
        if (fineNode == null || !fineNode.walkable)
        {
            return null;
        }
        
        Cluster cluster = grid.GetClusterAtPoint(worldPos);
        if (cluster == null)
        {
            return null;
        }
        
        // 创建临时入口点
        EntrancePoint tempEntrance = new EntrancePoint(new EntrancePointData
        {
            id = tempId,
            fineGridPos = new Vector2Int(fineNode.gridX, fineNode.gridY),
            worldPosition = fineNode.worldPosition,
            cluster1Id = cluster.id,
            cluster2Id = -1,
            isInter = false
        });
        
        return tempEntrance;
    }
    
    void RemoveTemporaryNodes()
    {
        // 临时节点不需要实际移除，因为没有加入到 grid 的字典中
        tempStartId = -1000;
        tempTargetId = -2000;
    }
    
    /// <summary>
    /// 在抽象图上寻路
    /// </summary>
    List<EntrancePoint> FindAbstractPath(EntrancePoint start, EntrancePoint target)
    {
        Dictionary<int, float> gCost = new Dictionary<int, float>();
        Dictionary<int, float> fCost = new Dictionary<int, float>();
        Dictionary<int, int> parent = new Dictionary<int, int>();
        HashSet<int> closedSet = new HashSet<int>();
        
        // 优先队列（简单实现）
        List<int> openSet = new List<int>();
        
        gCost[start.id] = 0;
        fCost[start.id] = Heuristic(start, target);
        openSet.Add(start.id);
        
        // 临时连接起点到簇内所有入口点
        Dictionary<int, float> startConnections = ConnectToClusterEntrances(start);
        Dictionary<int, float> targetConnections = ConnectToClusterEntrances(target);
        
        while (openSet.Count > 0)
        {
            // 找 fCost 最小的
            int currentId = openSet[0];
            float minF = fCost[currentId];
            
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fCost[openSet[i]] < minF)
                {
                    minF = fCost[openSet[i]];
                    currentId = openSet[i];
                }
            }
            
            openSet.Remove(currentId);
            closedSet.Add(currentId);
            
            if (currentId == target.id)
            {
                return RetraceAbstractPath(start, target, parent);
            }
            
            // 获取邻居
            List<AbstractEdge> neighbours = GetNeighbours(currentId, startConnections, targetConnections);
            
            foreach (var edge in neighbours)
            {
                int neighbourId = edge.toId;
                
                if (closedSet.Contains(neighbourId))
                    continue;
                
                float tentativeG = gCost[currentId] + edge.cost;
                
                if (!gCost.ContainsKey(neighbourId) || tentativeG < gCost[neighbourId])
                {
                    gCost[neighbourId] = tentativeG;
                    
                    EntrancePoint neighbourEP = GetEntrancePointById(neighbourId, start, target);
                    fCost[neighbourId] = tentativeG + Heuristic(neighbourEP, target);
                    
                    parent[neighbourId] = currentId;
                    
                    if (!openSet.Contains(neighbourId))
                    {
                        openSet.Add(neighbourId);
                    }
                }
            }
        }
        
        return null;
    }
    
    Dictionary<int, float> ConnectToClusterEntrances(EntrancePoint tempNode)
    {
        Dictionary<int, float> connections = new Dictionary<int, float>();
        
        Cluster cluster = grid.GetClusterAtPoint(tempNode.worldPosition);
        if (cluster == null) return connections;
        
        AStarNode tempFineNode = grid.FineNodeFromWorldPoint(tempNode.worldPosition);
        if (tempFineNode == null) return connections;
        
        foreach (int epId in cluster.entrancePointIds)
        {
            EntrancePoint ep = grid.GetEntrancePoint(epId);
            if (ep == null) continue;
            
            AStarNode epFineNode = grid.FineNodeFromWorldPoint(ep.worldPosition);
            if (epFineNode == null) continue;
            
            // 计算到该入口点的距离
            var path = FindFineAStarPath(tempFineNode, epFineNode);
            if (path != null)
            {
                float distance = CalculatePathLength(path);
                connections[epId] = distance;
            }
        }
        
        return connections;
    }
    
    List<AbstractEdge> GetNeighbours(int nodeId, Dictionary<int, float> startConn, Dictionary<int, float> targetConn)
    {
        List<AbstractEdge> neighbours = new List<AbstractEdge>();
        
        // 临时起点
        if (nodeId == tempStartId)
        {
            foreach (var kvp in startConn)
            {
                neighbours.Add(new AbstractEdge
                {
                    fromId = nodeId,
                    toId = kvp.Key,
                    cost = kvp.Value,
                    isInter = false
                });
            }
            return neighbours;
        }
        
        // 临时终点
        if (nodeId == tempTargetId)
        {
            return neighbours;  // 终点没有出边
        }
        
        // 正常节点
        var edges = grid.GetAbstractNeighbours(nodeId);
        neighbours.AddRange(edges);
        
        // 如果是目标簇内的节点，添加到目标的连接
        if (targetConn.ContainsKey(nodeId))
        {
            neighbours.Add(new AbstractEdge
            {
                fromId = nodeId,
                toId = tempTargetId,
                cost = targetConn[nodeId],
                isInter = false
            });
        }
        
        return neighbours;
    }
    
    EntrancePoint GetEntrancePointById(int id, EntrancePoint start, EntrancePoint target)
    {
        if (id == start.id) return start;
        if (id == target.id) return target;
        return grid.GetEntrancePoint(id);
    }
    
    float Heuristic(EntrancePoint a, EntrancePoint b)
    {
        return Vector3.Distance(a.worldPosition, b.worldPosition);
    }
    
    List<EntrancePoint> RetraceAbstractPath(EntrancePoint start, EntrancePoint target, Dictionary<int, int> parent)
    {
        List<EntrancePoint> path = new List<EntrancePoint>();
        int current = target.id;
        
        while (current != start.id)
        {
            EntrancePoint ep = GetEntrancePointById(current, start, target);
            if (ep != null)
            {
                path.Add(ep);
            }
            
            if (!parent.ContainsKey(current))
            {
                break;
            }
            
            current = parent[current];
        }
        
        path.Add(start);
        path.Reverse();
        
        return path;
    }
    
    /// <summary>
    /// 细化路径：连接抽象路径上的每两个点
    /// </summary>
    List<AStarNode> RefinePath(List<EntrancePoint> abstractPath)
    {
        if (abstractPath == null || abstractPath.Count == 0)
            return null;
        
        List<AStarNode> completePath = new List<AStarNode>();
        
        for (int i = 0; i < abstractPath.Count - 1; i++)
        {
            AStarNode from = grid.FineNodeFromWorldPoint(abstractPath[i].worldPosition);
            AStarNode to = grid.FineNodeFromWorldPoint(abstractPath[i + 1].worldPosition);
            
            if (from == null || to == null)
                continue;
            
            var segment = FindFineAStarPath(from, to);
            
            if (segment != null && segment.Count > 0)
            {
                if (i == 0)
                {
                    completePath.AddRange(segment);
                }
                else
                {
                    // 跳过第一个点（避免重复）
                    for (int j = 1; j < segment.Count; j++)
                    {
                        completePath.Add(segment[j]);
                    }
                }
            }
        }
        
        return completePath;
    }
    
    float CalculatePathLength(List<AStarNode> path)
    {
        float length = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector3.Distance(path[i].worldPosition, path[i + 1].worldPosition);
        }
        return length;
    }
    
    // === 路径处理 ===
    
    List<Vector3> ProcessPath(List<AStarNode> rawPath)
    {
        if (rawPath == null || rawPath.Count <= 2)
        {
            return ConvertToVector3List(rawPath);
        }
        
        List<Vector3> smoothedPath = SmoothPath(rawPath);
        
        if (enableCornerOptimization && smoothedPath != null)
        {
            smoothedPath = OptimizeCorners(smoothedPath);
        }
        
        return smoothedPath;
    }
    
    List<Vector3> SmoothPath(List<AStarNode> rawPath)
    {
        if (!enablePathSmoothing || rawPath == null || rawPath.Count <= 2)
        {
            return ConvertToVector3List(rawPath);
        }
        
        switch (smoothType)
        {
            case PathSmoothType.Raycast:
                return SmoothPathRaycast(rawPath);
            
            case PathSmoothType.CatmullRom:
                return SmoothPathCatmullRom(rawPath);
            
            case PathSmoothType.Simplified:
                return SimplifyPath(rawPath);
            
            case PathSmoothType.None:
            default:
                return ConvertToVector3List(rawPath);
        }
    }
    
    List<Vector3> SmoothPathRaycast(List<AStarNode> path)
    {
        if (path == null || path.Count <= 2)
        {
            return ConvertToVector3List(path);
        }
        
        List<Vector3> smoothPath = new List<Vector3>();
        smoothPath.Add(path[0].worldPosition);
        
        int currentIndex = 0;
        
        while (currentIndex < path.Count - 1)
        {
            int farthestIndex = currentIndex + 1;
            
            for (int i = currentIndex + 2; i < path.Count; i++)
            {
                if (IsPathClear(path[currentIndex], path[i]))
                {
                    farthestIndex = i;
                }
                else
                {
                    break;
                }
            }
            
            currentIndex = farthestIndex;
            smoothPath.Add(path[currentIndex].worldPosition);
        }
        
        return smoothPath;
    }
    
    List<Vector3> SmoothPathCatmullRom(List<AStarNode> path)
    {
        if (path == null || path.Count <= 2)
        {
            return ConvertToVector3List(path);
        }
        
        List<Vector3> keyPoints = SmoothPathRaycast(path);
        
        if (keyPoints.Count <= 2)
        {
            return keyPoints;
        }
        
        List<Vector3> smoothPath = new List<Vector3>();
        
        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? keyPoints[i] : keyPoints[i - 1];
            Vector3 p1 = keyPoints[i];
            Vector3 p2 = keyPoints[i + 1];
            Vector3 p3 = (i + 2 < keyPoints.Count) ? keyPoints[i + 2] : keyPoints[i + 1];
            
            for (int j = 0; j < catmullRomPointsPerSegment; j++)
            {
                float t = j / (float)catmullRomPointsPerSegment;
                Vector3 point = CalculateCatmullRomPoint(t, p0, p1, p2, p3);
                
                AStarNode node = grid.FineNodeFromWorldPoint(point);
                if (node != null && node.walkable)
                {
                    smoothPath.Add(point);
                }
                else
                {
                    smoothPath.Add(p1);
                    break;
                }
            }
        }
        
        smoothPath.Add(keyPoints[keyPoints.Count - 1]);
        
        return smoothPath;
    }
    
    List<Vector3> SimplifyPath(List<AStarNode> path, float angleThreshold = 5f)
    {
        if (path == null || path.Count <= 2)
        {
            return ConvertToVector3List(path);
        }
        
        List<Vector3> simplified = new List<Vector3>();
        simplified.Add(path[0].worldPosition);
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 dirToPrev = (path[i].worldPosition - path[i - 1].worldPosition).normalized;
            Vector3 dirToNext = (path[i + 1].worldPosition - path[i].worldPosition).normalized;
            
            float angle = Vector3.Angle(dirToPrev, dirToNext);
            
            if (angle > angleThreshold)
            {
                simplified.Add(path[i].worldPosition);
            }
        }
        
        simplified.Add(path[path.Count - 1].worldPosition);
        
        return simplified;
    }
    
    List<Vector3> OptimizeCorners(List<Vector3> path)
    {
        if (path == null || path.Count <= 2)
        {
            return path;
        }
        
        List<Vector3> optimizedPath = new List<Vector3>();
        optimizedPath.Add(path[0]);
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 prev = path[i - 1];
            Vector3 current = path[i];
            Vector3 next = path[i + 1];
            
            Vector3 dirFromPrev = (current - prev);
            dirFromPrev.y = 0;
            dirFromPrev.Normalize();
            
            Vector3 dirToNext = (next - current);
            dirToNext.y = 0;
            dirToNext.Normalize();
            
            float angle = Vector3.Angle(dirFromPrev, dirToNext);
            
            if (angle > minCornerAngle && angle < 180f - minCornerAngle)
            {
                Vector3 awayFromObstacles = CalculateAwayFromObstaclesDirection(current);
                
                if (awayFromObstacles != Vector3.zero)
                {
                    Vector3 offsetPoint = current + awayFromObstacles * cornerOffset;
                    
                    AStarNode offsetNode = grid.FineNodeFromWorldPoint(offsetPoint);
                    if (offsetNode != null && offsetNode.walkable)
                    {
                        optimizedPath.Add(offsetPoint);
                    }
                    else
                    {
                        float reducedOffset = cornerOffset * 0.5f;
                        offsetPoint = current + awayFromObstacles * reducedOffset;
                        offsetNode = grid.FineNodeFromWorldPoint(offsetPoint);
                        
                        if (offsetNode != null && offsetNode.walkable)
                        {
                            optimizedPath.Add(offsetPoint);
                        }
                        else
                        {
                            optimizedPath.Add(current);
                        }
                    }
                }
                else
                {
                    optimizedPath.Add(current);
                }
            }
            else
            {
                optimizedPath.Add(current);
            }
        }
        
        optimizedPath.Add(path[path.Count - 1]);
        
        return optimizedPath;
    }
    
    Vector3 CalculateAwayFromObstaclesDirection(Vector3 position)
    {
        AStarNode centerNode = grid.FineNodeFromWorldPoint(position);
        if (centerNode == null) return Vector3.zero;
        
        Vector3[] directions = new Vector3[]
        {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 1).normalized,
            new Vector3(-1, 0, 1).normalized,
            new Vector3(1, 0, -1).normalized,
            new Vector3(-1, 0, -1).normalized
        };
        
        Vector3 totalAwayDirection = Vector3.zero;
        int obstacleCount = 0;
        
        float checkRadius = grid.fineNodeRadius * 3;
        
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 checkPos = position + directions[i] * checkRadius;
            AStarNode checkNode = grid.FineNodeFromWorldPoint(checkPos);
            
            if (checkNode == null || !checkNode.walkable)
            {
                totalAwayDirection -= directions[i];
                obstacleCount++;
            }
        }
        
        if (obstacleCount > 0)
        {
            totalAwayDirection.Normalize();
            return totalAwayDirection;
        }
        
        return Vector3.zero;
    }
    
    bool IsPathClear(AStarNode from, AStarNode to)
    {
        Vector3 start = from.worldPosition;
        Vector3 end = to.worldPosition;
        
        float distance = Vector3.Distance(start, end);
        float nodeSize = grid.fineNodeRadius * 2;
        int sampleCount = Mathf.CeilToInt(distance / (nodeSize * 0.5f));
        
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Vector3 samplePoint = Vector3.Lerp(start, end, t);
            
            AStarNode node = grid.FineNodeFromWorldPoint(samplePoint);
            if (node == null || !node.walkable)
            {
                return false;
            }
        }
        
        return true;
    }
    
    Vector3 CalculateCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        
        Vector3 result = 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
        
        return result;
    }
    
    // === 细网格 A* ===
    
    List<AStarNode> FindFineAStarPath(AStarNode startNode, AStarNode targetNode)
    {
        MinHeap<AStarNode> openSet = new MinHeap<AStarNode>(grid.MaxFineSize);
        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();
        
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            AStarNode currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);
            
            if (currentNode == targetNode)
            {
                return RetraceFinePath(startNode, targetNode);
            }
            
            foreach (AStarNode neighbour in grid.GetFineNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                
                int newCost = currentNode.gCost + GetFineDistance(currentNode, neighbour);
                
                if (newCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCost;
                    neighbour.hCost = GetFineDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
        
        return null;
    }
    
    List<AStarNode> RetraceFinePath(AStarNode startNode, AStarNode endNode)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        return path;
    }
    
    int GetFineDistance(AStarNode nodeA, AStarNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    
    List<Vector3> ConvertToVector3List(List<AStarNode> nodes)
    {
        if (nodes == null) return null;
        
        List<Vector3> result = new List<Vector3>();
        foreach (var node in nodes)
        {
            result.Add(node.worldPosition);
        }
        return result;
    }
    
    public void DrawGizmos()
    {
        this.grid.DrawGizmos();
    }
}