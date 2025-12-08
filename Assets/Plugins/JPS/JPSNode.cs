// JPSNode.cs - 扩展版本
using UnityEngine;

public class JPSNode
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    
    public JPSNode parent;
    
    // JPS+ 预处理数据：8个方向的跳点距离
    public int[] jumpDistances = new int[8]; // 0=无跳点
    
    // 方向索引
    public const int DIR_N = 0;   // 北 (0, 1)
    public const int DIR_S = 1;   // 南 (0, -1)
    public const int DIR_E = 2;   // 东 (1, 0)
    public const int DIR_W = 3;   // 西 (-1, 0)
    public const int DIR_NE = 4;  // 东北 (1, 1)
    public const int DIR_NW = 5;  // 西北 (-1, 1)
    public const int DIR_SE = 6;  // 东南 (1, -1)
    public const int DIR_SW = 7;  // 西南 (-1, -1)
    
    public JPSNode(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        
        // 初始化跳点距离为0
        for (int i = 0; i < 8; i++)
        {
            jumpDistances[i] = 0;
        }
    }
    
    public static void GetDirectionVector(int dirIndex, out int dx, out int dy)
    {
        switch (dirIndex)
        {
            case DIR_N:  dx = 0;  dy = 1;  break;
            case DIR_S:  dx = 0;  dy = -1; break;
            case DIR_E:  dx = 1;  dy = 0;  break;
            case DIR_W:  dx = -1; dy = 0;  break;
            case DIR_NE: dx = 1;  dy = 1;  break;
            case DIR_NW: dx = -1; dy = 1;  break;
            case DIR_SE: dx = 1;  dy = -1; break;
            case DIR_SW: dx = -1; dy = -1; break;
            default:     dx = 0;  dy = 0;  break;
        }
    }
    
    public static int GetDirectionIndex(int dx, int dy)
    {
        if (dx == 0 && dy == 1) return DIR_N;
        if (dx == 0 && dy == -1) return DIR_S;
        if (dx == 1 && dy == 0) return DIR_E;
        if (dx == -1 && dy == 0) return DIR_W;
        if (dx == 1 && dy == 1) return DIR_NE;
        if (dx == -1 && dy == 1) return DIR_NW;
        if (dx == 1 && dy == -1) return DIR_SE;
        if (dx == -1 && dy == -1) return DIR_SW;
        return -1;
    }
}