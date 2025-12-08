using UnityEngine;
using System.Collections.Generic;

public class AStarGrid : MonoBehaviour
{
    [Header("Manual Obstacles")]
    [HideInInspector] public List<Vector2Int> manualObstacles = new List<Vector2Int>();
    
    public int MaxSize => gridSizeX * gridSizeY;
    
    public Vector2 gridWorldSize;
    public float nodeRadius;
    
    private AStarNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    
    public bool displayGridGizmos;
    public List<AStarNode> path;
    
    void Awake()
    {
        InitializeGrid();
    }
    
    void InitializeGrid()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }
    
    void CreateGrid()
    {
        grid = new AStarNode[gridSizeX, gridSizeY];
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
                grid[x, y] = new AStarNode(walkable, worldPoint, x, y);
            }
        }
    }
    
    public AStarNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        
        if (grid == null || x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            return null;
        }
        
        return grid[x, y];
    }
    
    public List<AStarNode> GetNeighbours(AStarNode node)
    {
        List<AStarNode> neighbours = new List<AStarNode>();
    
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
            
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
            
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    AStarNode neighbour = grid[checkX, checkY];
                
                    if (x != 0 && y != 0)
                    {
                        AStarNode side1 = grid[node.gridX + x, node.gridY];
                        AStarNode side2 = grid[node.gridX, node.gridY + y];
                    
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
    
    // 可视化绘制
    public void DrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
    
        if (displayGridGizmos)
        {
            float currentNodeDiameter = nodeRadius * 2;
            int currentGridSizeX = Mathf.RoundToInt(gridWorldSize.x / currentNodeDiameter);
            int currentGridSizeY = Mathf.RoundToInt(gridWorldSize.y / currentNodeDiameter);
        
            if (grid == null || gridSizeX != currentGridSizeX || gridSizeY != currentGridSizeY)
            {
                InitializeGrid();
            }
        }
    
        if (grid != null && displayGridGizmos)
        {
            foreach (AStarNode n in grid)
            {
                // 路径节点特殊处理
                if (this.displayGridGizmos && Application.isPlaying && path != null && path.Contains(n))
                {
                    // 先画半透明实心（显示路径）
                    Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                
                    // 再画黑色线框（清晰标记）
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
                }
                else
                {
                    Gizmos.color = n.walkable ? Color.white : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
    }
}