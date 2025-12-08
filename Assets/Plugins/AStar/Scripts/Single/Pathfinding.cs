using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    private AStarGrid grid;
    
    [Header("Path Smoothing")]
    public bool enablePathSmoothing = true;
    public PathSmoothType smoothType = PathSmoothType.Raycast;
    public int catmullRomPointsPerSegment = 5;
    
    [Header("Corner Optimization")]
    public bool enableCornerOptimization = true;
    [Range(0.1f, 2f)] public float cornerOffset = 0.5f; // 拐点偏移距离
    [Range(10f, 90f)] public float minCornerAngle = 30f; // 最小拐角角度
    
    public enum PathSmoothType
    {
        None,
        Raycast,
        CatmullRom,
        Simplified
    }
    
    void Awake()
    {
        grid = GetComponent<AStarGrid>();
    }
    
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        AStarNode startNode = grid.NodeFromWorldPoint(startPos);
        AStarNode targetNode = grid.NodeFromWorldPoint(targetPos);
        
        if (startNode == null || targetNode == null || !startNode.walkable || !targetNode.walkable)
        {
            return null;
        }
        
        MinHeap<AStarNode> openSet = new MinHeap<AStarNode>(grid.MaxSize);
        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            AStarNode currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);
            
            if (currentNode == targetNode)
            {
                List<AStarNode> rawPath = RetracePath(startNode, targetNode);
                grid.path = new List<AStarNode>(rawPath);
                return ProcessPath(rawPath);
            }
            
            foreach (AStarNode neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
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
    
    private List<AStarNode> RetracePath(AStarNode startNode, AStarNode endNode)
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
    
    private List<Vector3> ProcessPath(List<AStarNode> rawPath)
    {
        if (rawPath == null || rawPath.Count <= 2)
        {
            return PathSmoother.SimplifyPath(rawPath, 0f);
        }
        
        // 1️⃣ 先做路径平滑
        List<Vector3> smoothedPath = SmoothPath(rawPath);
        
        // 2️⃣ 再做拐点优化（在平滑后的路径上）
        if (enableCornerOptimization)
        {
            smoothedPath = PathSmoother.OptimizeCorners(smoothedPath, grid, cornerOffset, minCornerAngle);
        }
        
        return smoothedPath;
    }
    
    private List<Vector3> SmoothPath(List<AStarNode> rawPath)
    {
        if (!enablePathSmoothing || rawPath == null || rawPath.Count <= 2)
        {
            return PathSmoother.SimplifyPath(rawPath, 0f);
        }
        
        switch (smoothType)
        {
            case PathSmoothType.Raycast:
                return PathSmoother.SmoothPathRaycast(rawPath, grid);
            
            case PathSmoothType.CatmullRom:
                return PathSmoother.SmoothPathCatmullRom(rawPath, grid, catmullRomPointsPerSegment);
            
            case PathSmoothType.Simplified:
                return PathSmoother.SimplifyPath(rawPath, 5f);
            
            case PathSmoothType.None:
            default:
                List<Vector3> result = new List<Vector3>();
                foreach (var node in rawPath)
                {
                    result.Add(node.worldPosition);
                }
                return result;
        }
    }
    
    private int GetDistance(AStarNode nodeA, AStarNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void DrawGizmos()
    {
        this.grid.DrawGizmos();
    }
}
