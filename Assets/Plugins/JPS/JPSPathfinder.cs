using System.Collections.Generic;
using UnityEngine;

public partial class JPSPathfinder : MonoBehaviour
{
    [Header("References")]
    public JPSGrid grid;

    [Header("Optimization")]
    [Tooltip("最大搜索节点数,防止卡死")]
    public int maxSearchNodes = 5000;

    [Header("Step Debug")]
    public bool stepDebugMode = false;

    public JPSNode currentExploringNode;
    public List<JPSNode> currentCheckingNeighbors = new List<JPSNode>();
    protected List<JPSNode> openList;
    protected HashSet<JPSNode> closedSet;
    protected List<(Vector3 start, Vector3 end)> currentJumpPaths = new List<(Vector3, Vector3)>();
    public List<JPSNode> ExploredNodes { get; private set; }
    public List<JPSNode> JumpPoints { get; private set; }
    public Dictionary<JPSNode, List<JPSNode>> ForcedNeighborsMap { get; private set; }
    private bool isPaused = false;
    private bool stepOnce = false;
    private JPSNode startNode;
    private JPSNode targetNode;
    private int searchedNodes = 0;
    private List<JPSNode> entryJumpPoints = new List<JPSNode>();
    
    void Awake()
    {
        if (grid == null)
            grid = FindObjectOfType<JPSGrid>();

        openList = new List<JPSNode>();
        closedSet = new HashSet<JPSNode>();
        ExploredNodes = new List<JPSNode>();
        JumpPoints = new List<JPSNode>();
        ForcedNeighborsMap = new Dictionary<JPSNode, List<JPSNode>>();
    }

    public List<JPSNode> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        return FindPathStandardJPS(startPos, targetPos);
    }

    private List<JPSNode> FindPathStandardJPS(Vector3 startPos, Vector3 targetPos)
    {
        startNode = grid.NodeFromWorldPoint(startPos);
        targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.walkable || !targetNode.walkable)
        {
            Debug.LogWarning("起点或终点无效");
            return null;
        }

        ResetNodeCosts();

        openList.Clear();
        closedSet.Clear();
        ExploredNodes.Clear();
        JumpPoints.Clear();
        ForcedNeighborsMap.Clear();

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        openList.Add(startNode);

        searchedNodes = 0;

        while (openList.Count > 0)
        {
            if (++searchedNodes > maxSearchNodes)
            {
                Debug.LogWarning($"JPS搜索超过最大节点数 {maxSearchNodes},提前终止");
                return null;
            }

            JPSNode current = GetLowestFCostNode();
            openList.Remove(current);
            closedSet.Add(current);
            ExploredNodes.Add(current);

            if (current == targetNode)
            {
                var path = RetracePath(startNode, targetNode);
                grid.path = path;
                grid.exploredNodes = ExploredNodes;
                grid.jumpPoints = JumpPoints;
                return path;
            }

            List<JPSNode> neighbors = grid.GetNeighbors(current, current.parent);
            foreach (JPSNode neighbor in neighbors)
            {
                JPSNode jumpPoint = Jump(neighbor, current, targetNode);

                if (jumpPoint != null && !closedSet.Contains(jumpPoint))
                {
                    float newGCost = current.gCost + GetDistance(current, jumpPoint);

                    if (newGCost < jumpPoint.gCost)
                    {
                        jumpPoint.gCost = newGCost;
                        jumpPoint.hCost = GetDistance(jumpPoint, targetNode);
                        jumpPoint.parent = current;

                        if (!JumpPoints.Contains(jumpPoint))
                        {
                            JumpPoints.Add(jumpPoint);
                        }

                        if (!openList.Contains(jumpPoint))
                        {
                            openList.Add(jumpPoint);
                        }
                    }
                }
            }
        }

        Debug.LogWarning("未找到路径");
        return null;
    }

    protected List<JPSNode> RetracePathFromVirtualStart(JPSNode virtualStart, JPSNode endNode)
    {
        List<JPSNode> path = new List<JPSNode>();
        JPSNode currentNode = endNode;
        
        while (currentNode != null && currentNode != virtualStart)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
    
    private void ResetNodeCosts()
    {
        for (int x = 0; x < grid.GridSizeX; x++)
        {
            for (int y = 0; y < grid.GridSizeY; y++)
            {
                JPSNode node = grid.GetNode(x, y);
                if (node != null)
                {
                    node.gCost = float.MaxValue;
                    node.hCost = 0;
                    node.parent = null;
                }
            }
        }
    }
    
    #region Step Debugging
    public void StartPathfinding(Vector3 startPos, Vector3 targetPos)
    {
        startNode = grid.NodeFromWorldPoint(startPos);
        targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.walkable || !targetNode.walkable)
        {
            return;
        }

        ResetNodeCosts();

        openList.Clear();
        closedSet.Clear();
        ExploredNodes.Clear();
        JumpPoints.Clear();
        ForcedNeighborsMap.Clear();
        currentJumpPaths.Clear(); // ✅ 新寻路开始时清除历史路径
        entryJumpPoints.Clear();

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        openList.Add(startNode);

        searchedNodes = 0;
        isPaused = stepDebugMode;
    }
    
    public void StepOnce()
    {
        stepOnce = true;
        // ✅ 不清除历史路径，累积显示
    }
    
    void Update()
    {
        if (!stepDebugMode || !isPaused || !stepOnce) return;

        stepOnce = false;

        if (openList.Count == 0) return;

        if (++searchedNodes > maxSearchNodes)
        {
            Debug.LogWarning($"JPS搜索超过最大节点数 {maxSearchNodes},提前终止");
            isPaused = false;
            return;
        }

        currentExploringNode = GetLowestFCostNode();
        openList.Remove(currentExploringNode);
        closedSet.Add(currentExploringNode);
        ExploredNodes.Add(currentExploringNode);

        if (currentExploringNode == targetNode)
        {
            var nodePath = RetracePath(startNode, targetNode);
            grid.path = nodePath;
            grid.exploredNodes = ExploredNodes;
            grid.jumpPoints = JumpPoints;
            isPaused = false;
            Debug.Log("找到路径!");
            return;
        }

        currentCheckingNeighbors = grid.GetNeighbors(currentExploringNode, currentExploringNode.parent);

        foreach (JPSNode neighbor in currentCheckingNeighbors)
        {
            JPSNode jumpPoint = Jump(neighbor, currentExploringNode, targetNode);

            if (jumpPoint != null && !closedSet.Contains(jumpPoint))
            {
                float newGCost = currentExploringNode.gCost + GetDistance(currentExploringNode, jumpPoint);

                if (newGCost < jumpPoint.gCost)
                {
                    jumpPoint.gCost = newGCost;
                    jumpPoint.hCost = GetDistance(jumpPoint, targetNode);
                    jumpPoint.parent = currentExploringNode;

                    if (!JumpPoints.Contains(jumpPoint))
                    {
                        JumpPoints.Add(jumpPoint);
                        entryJumpPoints.Add(jumpPoint);
                    }

                    if (!openList.Contains(jumpPoint))
                    {
                        openList.Add(jumpPoint);
                    }
                }
            }
        }
    }
    #endregion
    
    /// <summary>
    /// JPS核心：递归跳跃函数（带切角检测）
    /// </summary>
    protected JPSNode Jump(JPSNode neighbor, JPSNode current, JPSNode target)
    {
        if (neighbor == null || !neighbor.walkable)
            return null;

        int dx = neighbor.gridX - current.gridX;
        int dy = neighbor.gridY - current.gridY;

        // ✅ 在stepDebugMode下，立即记录每一步的跳跃路径（实现连续箭头）
        if (stepDebugMode)
        {
            currentJumpPaths.Add((current.worldPosition, neighbor.worldPosition));
        }

        // ✅ 切角检测：在函数开始时检查从current到neighbor的切角
        if (dx != 0 && dy != 0)
        {
            // 检查两个相邻方向（相对于current）
            bool horzWalkable = grid.IsWalkable(current.gridX + dx, current.gridY);
            bool vertWalkable = grid.IsWalkable(current.gridX, current.gridY + dy);
            
            // 两个都不可通行 → 切角，阻止
            if (!horzWalkable && !vertWalkable)
            {
                return null;
            }
        }

        // ✅ 到达目标
        if (neighbor == target)
        {
            if (stepDebugMode)
            {
                RecordForcedNeighborsImmediate(neighbor, dx, dy);
            }
            return neighbor;
        }

        // ✅ 强制邻居检测
        if (HasForcedNeighbors(neighbor, dx, dy))
        {
            if (stepDebugMode)
            {
                RecordForcedNeighborsImmediate(neighbor, dx, dy);
            }
            return neighbor;
        }

        // ✅ 对角线：递归检查水平和垂直方向
        if (dx != 0 && dy != 0)
        {
            JPSNode horzNode = grid.GetNode(neighbor.gridX + dx, neighbor.gridY);
            JPSNode vertNode = grid.GetNode(neighbor.gridX, neighbor.gridY + dy);

            if (horzNode != null && horzNode.walkable)
            {
                if (Jump(horzNode, neighbor, target) != null)
                {
                    if (stepDebugMode)
                    {
                        RecordForcedNeighborsImmediate(neighbor, dx, dy);
                    }
                    return neighbor;
                }
            }

            if (vertNode != null && vertNode.walkable)
            {
                if (Jump(vertNode, neighbor, target) != null)
                {
                    if (stepDebugMode)
                    {
                        RecordForcedNeighborsImmediate(neighbor, dx, dy);
                    }
                    return neighbor;
                }
            }
        }
        
        // 递归跳跃到下一个节点
        return Jump(grid.GetNode(neighbor.gridX + dx, neighbor.gridY + dy), neighbor, target);
    }

    protected bool HasForcedNeighbors(JPSNode node, int dx, int dy)
    {
        if (dx != 0 && dy != 0) // 对角线
        {
            if ((!grid.IsWalkable(node.gridX - dx, node.gridY) && grid.IsWalkable(node.gridX - dx, node.gridY + dy)) ||
                (!grid.IsWalkable(node.gridX, node.gridY - dy) && grid.IsWalkable(node.gridX + dx, node.gridY - dy)))
            {
                return true;
            }
        }
        else if (dx != 0) // 水平
        {
            if ((!grid.IsWalkable(node.gridX, node.gridY + 1) && grid.IsWalkable(node.gridX + dx, node.gridY + 1)) ||
                (!grid.IsWalkable(node.gridX, node.gridY - 1) && grid.IsWalkable(node.gridX + dx, node.gridY - 1)))
            {
                return true;
            }
        }
        else if (dy != 0) // 垂直
        {
            if ((!grid.IsWalkable(node.gridX + 1, node.gridY) && grid.IsWalkable(node.gridX + 1, node.gridY + dy)) ||
                (!grid.IsWalkable(node.gridX - 1, node.gridY) && grid.IsWalkable(node.gridX - 1, node.gridY + dy)))
            {
                return true;
            }
        }
        return false;
    }
    
    protected void RecordForcedNeighborsImmediate(JPSNode node, int dx, int dy)
    {
        if (!ForcedNeighborsMap.ContainsKey(node))
        {
            ForcedNeighborsMap[node] = new List<JPSNode>();
        }

        List<JPSNode> forcedList = ForcedNeighborsMap[node];

        if (dx != 0 && dy != 0) // 对角线
        {
            if (!grid.IsWalkable(node.gridX - dx, node.gridY) && grid.IsWalkable(node.gridX - dx, node.gridY + dy))
            {
                JPSNode forced = grid.GetNode(node.gridX - dx, node.gridY + dy);
                if (forced != null && !forcedList.Contains(forced))
                    forcedList.Add(forced);
            }

            if (!grid.IsWalkable(node.gridX, node.gridY - dy) && grid.IsWalkable(node.gridX + dx, node.gridY - dy))
            {
                JPSNode forced = grid.GetNode(node.gridX + dx, node.gridY - dy);
                if (forced != null && !forcedList.Contains(forced))
                    forcedList.Add(forced);
            }
        }
        else if (dx != 0) // 水平
        {
            if (!grid.IsWalkable(node.gridX, node.gridY + 1) && grid.IsWalkable(node.gridX + dx, node.gridY + 1))
            {
                JPSNode forced = grid.GetNode(node.gridX + dx, node.gridY + 1);
                if (forced != null && !forcedList.Contains(forced))
                    forcedList.Add(forced);
            }

            if (!grid.IsWalkable(node.gridX, node.gridY - 1) && grid.IsWalkable(node.gridX + dx, node.gridY - 1))
            {
                JPSNode forced = grid.GetNode(node.gridX + dx, node.gridY - 1);
                if (forced != null && !forcedList.Contains(forced))
                    forcedList.Add(forced);
            }
        }
        else if (dy != 0) // 垂直
        {
            if (!grid.IsWalkable(node.gridX + 1, node.gridY) && grid.IsWalkable(node.gridX + 1, node.gridY + dy))
            {
                JPSNode forced = grid.GetNode(node.gridX + 1, node.gridY + dy);
                if (forced != null && !forcedList.Contains(forced))
                    forcedList.Add(forced);
            }

            if (!grid.IsWalkable(node.gridX - 1, node.gridY) && grid.IsWalkable(node.gridX - 1, node.gridY + dy))
            {
                JPSNode forced = grid.GetNode(node.gridX - 1, node.gridY + dy);
                if (forced != null && !forcedList.Contains(forced))
                    forcedList.Add(forced);
            }
        }
    }
    
    protected bool IsTargetInDirection(JPSNode current, JPSNode target, int dx, int dy)
    {
        if (dx != 0 && dy == 0) // 水平
        {
            if (current.gridY == target.gridY)
            {
                int sign = dx > 0 ? 1 : -1;
                return sign * (target.gridX - current.gridX) > 0;
            }
        }
        else if (dx == 0 && dy != 0) // 垂直
        {
            if (current.gridX == target.gridX)
            {
                int sign = dy > 0 ? 1 : -1;
                return sign * (target.gridY - current.gridY) > 0;
            }
        }
        else if (dx != 0 && dy != 0) // 对角线
        {
            int signX = dx > 0 ? 1 : -1;
            int signY = dy > 0 ? 1 : -1;

            bool xMatch = signX * (target.gridX - current.gridX) > 0;
            bool yMatch = signY * (target.gridY - current.gridY) > 0;

            if (xMatch && yMatch)
            {
                int diffX = Mathf.Abs(target.gridX - current.gridX);
                int diffY = Mathf.Abs(target.gridY - current.gridY);
                return diffX == diffY;
            }
        }
        return false;
    }
    
    protected bool IsPathClearInDirection(JPSNode start, JPSNode end, int dx, int dy, int maxSteps)
    {
        int x = start.gridX;
        int y = start.gridY;
        
        for (int step = 1; step <= maxSteps; step++)
        {
            x += dx;
            y += dy;
            
            if (!grid.IsWalkable(x, y))
                return false;
                
            // ✅ 对角线移动切角检测
            if (dx != 0 && dy != 0)
            {
                bool horzWalkable = grid.IsWalkable(x - dx, y);
                bool vertWalkable = grid.IsWalkable(x, y - dy);
                
                if (!horzWalkable && !vertWalkable)
                    return false;
            }
            
            if (x == end.gridX && y == end.gridY)
                return true;
        }
        
        return false;
    }

    protected bool IsPathClear(JPSNode start, JPSNode end)
    {
        int dx = end.gridX > start.gridX ? 1 : (end.gridX < start.gridX ? -1 : 0);
        int dy = end.gridY > start.gridY ? 1 : (end.gridY < start.gridY ? -1 : 0);
        
        int x = start.gridX;
        int y = start.gridY;
        
        while (x != end.gridX || y != end.gridY)
        {
            x += dx;
            y += dy;
            
            if (!grid.IsWalkable(x, y))
                return false;
                
            // ✅ 对角线移动切角检测
            if (dx != 0 && dy != 0)
            {
                bool horzWalkable = grid.IsWalkable(x - dx, y);
                bool vertWalkable = grid.IsWalkable(x, y - dy);
                
                if (!horzWalkable && !vertWalkable)
                    return false;
            }
        }
        
        return true;
    }
    
    protected float GetDistance(JPSNode a, JPSNode b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);
        
        if (distX > distY)
            return 1.4f * distY + (distX - distY);
        return 1.4f * distX + (distY - distX);
    }

    protected List<JPSNode> RetracePath(JPSNode startNode, JPSNode endNode)
    {
        List<JPSNode> path = new List<JPSNode>();
        JPSNode currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        path.Add(startNode);
        path.Reverse();
        
        return path;
    }
    
    protected JPSNode GetLowestFCostNode()
    {
        JPSNode lowestNode = openList[0];
        for (int i = 1; i < openList.Count; i++)
        {
            if (openList[i].fCost < lowestNode.fCost ||
                (openList[i].fCost == lowestNode.fCost && openList[i].hCost < lowestNode.hCost))
            {
                lowestNode = openList[i];
            }
        }
        return lowestNode;
    }
    
    public void DrawGizmos()
    {
        // ✅ 始终绘制网格
        grid.DrawGizmos();

        if (!Application.isPlaying) return;

        // ✅ 1. 绘制探索节点的连接线（绿色）
        if (ExploredNodes != null && ExploredNodes.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (JPSNode node in ExploredNodes)
            {
                JPSNode current = node;
                while (current.parent != null)
                {
                    Gizmos.DrawLine(current.parent.worldPosition + Vector3.up * 0.15f,
                        current.worldPosition + Vector3.up * 0.15f);
                    current = current.parent;
                }
            }
        }

        // ✅ 2. 实时显示跳点（青色球体）- 单步调试时使用JPSPathfinder.JumpPoints
        if (JumpPoints != null && JumpPoints.Count > 0)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
            foreach (JPSNode jp in JumpPoints)
            {
                Gizmos.DrawSphere(jp.worldPosition + Vector3.up * 0.1f, grid.nodeRadius * 0.6f);
            }
        }

        // ✅ 3. 绘制强制邻居（红色箭头）
        if (ForcedNeighborsMap != null && ForcedNeighborsMap.Count > 0)
        {
            foreach (var kvp in ForcedNeighborsMap)
            {
                JPSNode jumpPoint = kvp.Key;
                List<JPSNode> forcedNeighbors = kvp.Value;

                foreach (JPSNode forced in forcedNeighbors)
                {
                    Gizmos.color = Color.red;
                    Vector3 start = jumpPoint.worldPosition + Vector3.up * 0.2f;
                    Vector3 end = forced.worldPosition + Vector3.up * 0.2f;
                    Gizmos.DrawLine(start, end);

                    // 绘制箭头
                    Vector3 direction = (end - start).normalized;
                    Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
                    Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;
                    Gizmos.DrawRay(end, right * 0.3f);
                    Gizmos.DrawRay(end, left * 0.3f);

                    // 强制邻居红色球体
                    Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
                    Gizmos.DrawSphere(forced.worldPosition + Vector3.up * 0.1f, grid.nodeRadius * 0.4f);
                }
            }
        }

        // ✅ 4. 单步调试专用可视化
        if (stepDebugMode)
        {
            // 当前探索节点（橙色大球）
            if (currentExploringNode != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
                Gizmos.DrawWireSphere(currentExploringNode.worldPosition + Vector3.up * 0.1f, grid.nodeRadius * 1.3f);
                Gizmos.DrawSphere(currentExploringNode.worldPosition + Vector3.up * 0.1f, grid.nodeRadius * 0.8f);
            }

            // 跳跃路径（紫色箭头，累积显示）
            if (currentJumpPaths.Count > 0)
            {
                Gizmos.color = new Color(1f, 0f, 1f, 0.7f);
                foreach (var (start, end) in currentJumpPaths)
                {
                    Gizmos.DrawLine(start + Vector3.up * 0.25f, end + Vector3.up * 0.25f);

                    // 绘制箭头
                    Vector3 direction = (end - start).normalized;
                    Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
                    Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;
                    Gizmos.DrawRay(end + Vector3.up * 0.25f, right * 0.3f);
                    Gizmos.DrawRay(end + Vector3.up * 0.25f, left * 0.3f);
                }
            }

            // 起点（蓝色方块）
            if (startNode != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(startNode.worldPosition + Vector3.up * 0.15f, Vector3.one * grid.nodeRadius * 0.8f);
            }

            // 终点（洋红色方块）
            if (targetNode != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(targetNode.worldPosition + Vector3.up * 0.15f, Vector3.one * grid.nodeRadius * 0.8f);
            }

            // 本步新发现的跳点（紫色方框）
            if (entryJumpPoints.Count > 0)
            {
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.8f);
                foreach (var jp in entryJumpPoints)
                {
                    Gizmos.DrawWireCube(jp.worldPosition + Vector3.up * 0.1f, Vector3.one * grid.nodeRadius * 2.2f);
                }
                entryJumpPoints.Clear();
            }
        }
    }
}