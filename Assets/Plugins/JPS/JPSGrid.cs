using System;
using UnityEngine;
using System.Collections.Generic;

public class JPSGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 0.5f;
    
    [Header("Visualization")]
    public bool displayGridGizmos = true;
    
    [Header("Manual Obstacles")]
    [HideInInspector] public List<Vector2Int> manualObstacles = new List<Vector2Int>();
    
    public List<JPSNode> path;
    public List<JPSNode> exploredNodes = new List<JPSNode>();
    public List<JPSNode> jumpPoints = new List<JPSNode>();
    
    private JPSNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    
    public int MaxSize => gridSizeX * gridSizeY;
    public int GridSizeX => gridSizeX;
    public int GridSizeY => gridSizeY;
    
    void Awake()
    {
        InitializeGrid();
    }
    
    public void InitializeGrid()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }
    
    void CreateGrid()
    {
        grid = new JPSNode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position 
            - Vector3.right * gridWorldSize.x / 2 
            - Vector3.forward * gridWorldSize.y / 2;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft 
                    + Vector3.right * (x * nodeDiameter + nodeRadius) 
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);
                
                bool walkable = !IsManualObstacle(x, y);
                grid[x, y] = new JPSNode(walkable, worldPoint, x, y);
            }
        }
    }
   
    public JPSNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null)
        {
            return null;
        }

        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2) / gridWorldSize.y;

        if (percentX < 0 || percentX > 1 || percentY < 0 || percentY > 1)
        {
            return null;
        }

        int x = Mathf.FloorToInt(gridSizeX * percentX);
        int y = Mathf.FloorToInt(gridSizeY * percentY);

        if (x == gridSizeX)
        {
            x = gridSizeX - 1;
        }
        if (y == gridSizeY)
        {
            y = gridSizeY - 1;
        }

        return grid[x, y];
    }
    
    public JPSNode GetNode(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return null;
        return grid[x, y];
    }
    
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return false;
        return grid[x, y].walkable;
    }
    
    public void SetObstacle(int x, int y, bool isObstacle)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (isObstacle)
        {
            if (!manualObstacles.Contains(pos))
                manualObstacles.Add(pos);
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
    
    /// <summary>
    /// 获取邻居节点（带JPS剪枝和切角检测）
    /// </summary>
    public List<JPSNode> GetNeighbors(JPSNode node, JPSNode parent)
    {
        List<JPSNode> neighbors = new List<JPSNode>();
        
        if (parent == null)
        {
            // ✅ 起点：返回所有8个方向的可通行邻居（带切角检测）
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    int checkX = node.gridX + dx;
                    int checkY = node.gridY + dy;
                    
                    if (checkX < 0 || checkX >= gridSizeX || checkY < 0 || checkY >= gridSizeY)
                        continue;
                    
                    JPSNode neighbor = grid[checkX, checkY];
                    if (!neighbor.walkable) continue;
                    
                    // ✅ 对角线移动：必须至少有一个相邻方向可通行
                    if (dx != 0 && dy != 0)
                    {
                        bool horzWalkable = IsWalkable(node.gridX + dx, node.gridY);
                        bool vertWalkable = IsWalkable(node.gridX, node.gridY + dy);
                        
                        // 两个相邻方向都不可通行 → 切角，不允许
                        if (!horzWalkable && !vertWalkable)
                            continue;
                    }
                    
                    neighbors.Add(neighbor);
                }
            }
        }
        else
        {
            // JPS剪枝逻辑
            int dx = Mathf.Clamp(node.gridX - parent.gridX, -1, 1);
            int dy = Mathf.Clamp(node.gridY - parent.gridY, -1, 1);
            
            if (dx != 0 && dy != 0) // 对角线移动
            {
                // 1. 自然邻居
                // ✅ 对角线方向需要切角检测
                if (CanMoveDiagonal(node, dx, dy))
                {
                    AddNeighborIfValid(neighbors, node.gridX + dx, node.gridY + dy);
                }
                AddNeighborIfValid(neighbors, node.gridX + dx, node.gridY); // 水平
                AddNeighborIfValid(neighbors, node.gridX, node.gridY + dy); // 垂直
                
                // 2. 强制邻居
                if (!IsWalkable(node.gridX - dx, node.gridY))
                    AddNeighborIfValid(neighbors, node.gridX - dx, node.gridY + dy);
                if (!IsWalkable(node.gridX, node.gridY - dy))
                    AddNeighborIfValid(neighbors, node.gridX + dx, node.gridY - dy);
            }
            else // 直线移动
            {
                if (dx != 0) // 水平
                {
                    AddNeighborIfValid(neighbors, node.gridX + dx, node.gridY);
                    if (!IsWalkable(node.gridX, node.gridY + 1))
                        AddNeighborIfValid(neighbors, node.gridX + dx, node.gridY + 1);
                    if (!IsWalkable(node.gridX, node.gridY - 1))
                        AddNeighborIfValid(neighbors, node.gridX + dx, node.gridY - 1);
                }
                else // 垂直
                {
                    AddNeighborIfValid(neighbors, node.gridX, node.gridY + dy);
                    if (!IsWalkable(node.gridX + 1, node.gridY))
                        AddNeighborIfValid(neighbors, node.gridX + 1, node.gridY + dy);
                    if (!IsWalkable(node.gridX - 1, node.gridY))
                        AddNeighborIfValid(neighbors, node.gridX - 1, node.gridY + dy);
                }
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// 检查是否可以沿对角线移动（至少有一个相邻方向可通行）
    /// </summary>
    private bool CanMoveDiagonal(JPSNode from, int dx, int dy)
    {
        // 对角线移动需要至少一个相邻直线方向可通行
        bool horzWalkable = IsWalkable(from.gridX + dx, from.gridY);
        bool vertWalkable = IsWalkable(from.gridX, from.gridY + dy);
        
        return horzWalkable || vertWalkable;
    }
    
    private void AddNeighborIfValid(List<JPSNode> neighbors, int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && grid[x, y].walkable)
            neighbors.Add(grid[x, y]);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            this.DrawGizmos();
        }
    }

    public void DrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        
        if (displayGridGizmos)
        {
            float currentNodeDiameter = nodeRadius * 2;
            int currentGridSizeX = Mathf.RoundToInt(gridWorldSize.x / currentNodeDiameter);
            int currentGridSizeY = Mathf.RoundToInt(gridWorldSize.y / currentNodeDiameter);
            
            if (grid == null || gridSizeX != currentGridSizeX || gridSizeY != currentGridSizeY)
                InitializeGrid();
        }
        
        if (grid != null && displayGridGizmos)
        {
            foreach (JPSNode node in grid)
            {
                if (path != null && path.Contains(node))
                {
                    Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
                }
                // ✅ 运行时不在这里绘制jumpPoints，由JPSPathfinder统一管理实时显示
                else if (exploredNodes != null && exploredNodes.Contains(node))
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
                else if (!node.walkable)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
                else
                {
                    Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
    }
}