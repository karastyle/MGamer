// 放置在 Assets/Scripts/JPSBakedData.cs
using UnityEngine;

namespace JPSPlus
{
// 放置在 Assets/Scripts/JPSBakedData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "JPSBakedData", menuName = "JPS/Baked Data")]
public class JPSBakedData : ScriptableObject
{
    public int gridWidth;
    public int gridHeight;
    public float nodeRadius;
    public Vector3 gridWorldOrigin; // 网格(0,0)的世界坐标（左下角）

    // 扁平化数组，存储8个方向的距离
    // 访问方式: bakedJumpDistances[ (y * gridWidth + x) * 8 + dirIndex ]
    [HideInInspector]
    public int[] bakedJumpDistances;
    
    // 扁平化数组，存储原始墙体数据
    [HideInInspector]
    public bool[] walls;
    
    // ===========================================
    //   !!!! 新增数据 !!!!
    // ===========================================
    // 存储每个格子的跳点标记 (来自 JPSBaker.BakerNode.jumpDirFlags)
    [HideInInspector]
    public byte[] bakedJumpPointFlags;
    // ===========================================

    /// <summary>
    /// 初始化烘焙数据
    /// </summary>
    public void Initialize(int width, int height, float radius, Vector3 origin, bool[] wallData)
    {
        gridWidth = width;
        gridHeight = height;
        nodeRadius = radius;
        gridWorldOrigin = origin;
        walls = wallData;
        
        // 8 个方向
        bakedJumpDistances = new int[width * height * 8];
        
        // !!! 新增 !!!
        bakedJumpPointFlags = new byte[width * height];
    }
    
    /// <summary>
    /// 获取指定格子指定方向的跳跃距离
    /// </summary>
    public int GetDistance(int x, int y, int dirIndex)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return 0;
        int index = (y * gridWidth + x) * 8 + dirIndex;
        return bakedJumpDistances[index];
    }

    /// <summary>
    /// 设置指定格子指定方向的跳跃距离
    /// </summary>
    public void SetDistance(int x, int y, int dirIndex, int distance)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;
        int index = (y * gridWidth + x) * 8 + dirIndex;
        bakedJumpDistances[index] = distance;
    }
    
    // ===========================================
    //   !!!! 新增的辅助方法 !!!!
    // ===========================================
    
    /// <summary>
    /// 设置指定格子的跳点标记
    /// </summary>
    public void SetJumpFlags(int x, int y, EDirFlags flags)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;
        bakedJumpPointFlags[y * gridWidth + x] = (byte)flags;
    }

    /// <summary>
    /// 获取指定格子的跳点标记
    /// </summary>
    public EDirFlags GetJumpFlags(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return EDirFlags.NONE;
        return (EDirFlags)bakedJumpPointFlags[y * gridWidth + x];
    }
    
    /// <summary>
    /// 检查一个格子是否是主要跳点
    /// </summary>
    public bool IsPrimaryJumpPoint(int x, int y)
    {
        return GetJumpFlags(x, y) != EDirFlags.NONE;
    }
    // ===========================================
    
    public bool IsWall(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return true;
        return walls[y * gridWidth + x];
    }
    
    public bool IsWalkable(int x, int y)
    {
        return !IsWall(x, y);
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        float nodeDiameter = nodeRadius * 2;
        return gridWorldOrigin + new Vector3(x * nodeDiameter + nodeRadius, 0, y * nodeDiameter + nodeRadius);
    }
    
    public Int2 GetGridCoords(Vector3 worldPosition)
    {
        float nodeDiameter = nodeRadius * 2;
        Vector3 localPos = worldPosition - gridWorldOrigin;
        int x = Mathf.FloorToInt(localPos.x / nodeDiameter);
        int y = Mathf.FloorToInt(localPos.z / nodeDiameter);
        return new Int2(x, y);
    }
}

}