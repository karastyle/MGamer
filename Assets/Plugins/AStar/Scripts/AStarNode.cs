using UnityEngine;

public interface IHeapItem<T> : System.IComparable<T>
{
    int HeapIndex { get; set; }
}

public class AStarNode : IHeapItem<AStarNode>
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public int gCost;
    public int hCost;
    public AStarNode parent;
    
    public int fCost => gCost + hCost;
    
    // MinHeap 需要的索引
    public int HeapIndex { get; set; }
    
    public AStarNode(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
    
    // 比较函数：fCost 小的优先级高
    public int CompareTo(AStarNode other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }
        return -compare; // 负号：让小的排前面（MinHeap）
    }
}