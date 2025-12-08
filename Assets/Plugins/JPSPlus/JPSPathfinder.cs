// 放置在 Assets/Scripts/JPSPathfinder.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JPSPlus
{
    public class JPSPathfinder
    {
        // 内部节点类
        private class PathNode : IHeapItem<PathNode>
        {
            public Int2 position;
            public int gCost;
            public int hCost;
            public PathNode parent;
            public EDirFlags fromDir; // 从哪个方向到达的

            public int FCost { get { return gCost + hCost; } }
            public int HeapIndex { get; set; }

            public PathNode(Int2 pos)
            {
                this.position = pos;
                gCost = int.MaxValue;
                hCost = 0;
                parent = null;
                fromDir = EDirFlags.NONE;
            }

            public int CompareTo(PathNode other)
            {
                int compare = FCost.CompareTo(other.FCost);
                if (compare == 0)
                {
                    compare = hCost.CompareTo(other.hCost);
                }

                return -compare; // 翻转，因为是最小堆
            }
        }

        private JPSBakedData mBakedMap;
        private Dictionary<Int2, PathNode> mCreatedNodes = new Dictionary<Int2, PathNode>();
        private Heap<PathNode> mOpenList;
        private HashSet<Int2> mCloseList = new HashSet<Int2>();

        private PathNode mStartNode;
        private PathNode mGoalNode;
        private Int2 mGoalPos;

        // (10 = 直线, 14 = 对角线)
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        
        // 确保遵循 JPS 规则，检查角点切割
        private const bool CHECK_CORNER_CUTTING = true; 

        public JPSPathfinder(JPSBakedData bakedData)
        {
            if (bakedData == null || bakedData.bakedJumpDistances == null)
            {
                Debug.LogError("JPSPathfinder: 烘焙数据无效!");
                return;
            }

            mBakedMap = bakedData;
            mOpenList = new Heap<PathNode>(mBakedMap.gridWidth * mBakedMap.gridHeight);
        }

        public bool FindPath(Vector3 startPos, Vector3 targetPos, List<Vector3> outPath)
        {
            outPath.Clear();
            if (mBakedMap == null) return false;

            // 1. 初始化
            Reset();

            mStartNode = GetOrCreateNode(mBakedMap.GetGridCoords(startPos));
            mGoalNode = GetOrCreateNode(mBakedMap.GetGridCoords(targetPos));
            mGoalPos = mGoalNode.position;

            if (mBakedMap.IsWall(mStartNode.position.x, mStartNode.position.y) ||
                mBakedMap.IsWall(mGoalNode.position.x, mGoalNode.position.y))
            {
                return false; // 起点或终点在墙内
            }

            // --- 优化: 检查起点到终点是否可直达 ---
            if (HasLineOfSight(mStartNode.position, mGoalPos))
            {
                outPath.Add(mBakedMap.GetWorldPosition(mStartNode.position.x, mStartNode.position.y));
                outPath.Add(mBakedMap.GetWorldPosition(mGoalNode.position.x, mGoalNode.position.y));
                return true;
            }
            // ----------------------------------------

            mStartNode.gCost = 0;
            mStartNode.hCost = H(mStartNode.position, mGoalPos);
            mStartNode.fromDir = EDirFlags.ALL; // 起点可以向所有方向
            mOpenList.Add(mStartNode);

            // 2. A* 循环
            while (mOpenList.Count > 0)
            {
                PathNode currNode = mOpenList.RemoveFirst();

                if (currNode.position == mGoalPos)
                {
                    RetracePath(currNode, outPath);
                    return true; // 找到路径
                }

                mCloseList.Add(currNode.position);

                FindSuccessors(currNode);
            }

            return false; // 未找到路径
        }

        private void FindSuccessors(PathNode currNode)
        {
            Int2 currPos = currNode.position;
            EDirFlags validDirs = ValidLookUPTable(currNode.fromDir);

            for (int i = 0; i < 8; i++)
            {
                EDirFlags processDir = DirFlags.FromArrayIndex(i);
                if ((processDir & validDirs) == EDirFlags.NONE)
                {
                    continue;
                }

                bool isDiagonalDir = DirFlags.IsDiagonal(processDir);
                int dirDistance = mBakedMap.GetDistance(currPos.x, currPos.y, i);
                int lengthX = Mathf.Abs(mGoalPos.x - currPos.x);
                int lengthY = Mathf.Abs(mGoalPos.y - currPos.y);

                PathNode nextNode = null;
                int nextGCost = 0;

                // --- 终点拦截检查 (已修正) ---

                // 1. 终点在当前象限内
                if (IsGoalInGeneralDirection(currPos, processDir, mGoalPos))
                {
                    // 计算到目标点的曼哈顿距离（用于比较）
                    int goalDistManhattan = Math.Max(lengthX, lengthY);

                    // 检查 1: 目标是否比下一个跳点 J 更近？ (|G| <= |J|)
                    if (dirDistance <= 0 || goalDistManhattan <= Mathf.Abs(dirDistance))
                    {
                        // 检查 2: 目标是否可以被当前节点直视 (Line-of-Sight)?
                        if (HasLineOfSight(currPos, mGoalPos))
                        {
                            nextNode = mGoalNode;
                            
                            // 更好的G值计算（对角线和直线混合）
                            int gDiag = Math.Min(lengthX, lengthY) * MOVE_DIAGONAL_COST;
                            int gStraight = (Math.Max(lengthX, lengthY) - Math.Min(lengthX, lengthY)) * MOVE_STRAIGHT_COST;
                            nextGCost = currNode.gCost + gDiag + gStraight;
                        }
                    }
                }
                
                // 2. 如果没有拦截到终点，则跳到预烘焙的跳点 (只有 dirDistance > 0 时才有效)
                if (nextNode == null && dirDistance > 0)
                {
                    Int2 jumpPos = currPos + (DirFlags.ToPos(processDir) * dirDistance);
                    nextNode = GetOrCreateNode(jumpPos);

                    int gDiag = Math.Min(Mathf.Abs(jumpPos.x - currPos.x), Mathf.Abs(jumpPos.y - currPos.y)) * MOVE_DIAGONAL_COST;
                    int gStraight =
                            (Math.Max(Mathf.Abs(jumpPos.x - currPos.x), Mathf.Abs(jumpPos.y - currPos.y)) -
                                Math.Min(Mathf.Abs(jumpPos.x - currPos.x), Mathf.Abs(jumpPos.y - currPos.y))) * MOVE_STRAIGHT_COST;
                    nextGCost = currNode.gCost + gDiag + gStraight;
                }
                else if (nextNode == null)
                {
                    // 既没有拦截终点，也没有预烘焙跳点，跳过
                    continue;
                }
                
                // 3. A* 逻辑更新
                if (nextNode == null || mCloseList.Contains(nextNode.position))
                {
                    continue;
                }

                if (nextGCost < nextNode.gCost)
                {
                    nextNode.parent = currNode;
                    nextNode.gCost = nextGCost;
                    nextNode.hCost = H(nextNode.position, mGoalPos);
                    nextNode.fromDir = processDir; // 记录我们是如何到达这个节点的

                    if (!mOpenList.Contains(nextNode))
                    {
                        mOpenList.Add(nextNode);
                    }
                    else
                    {
                        mOpenList.UpdateItem(nextNode);
                    }
                }
            }
        }

        private void Reset()
        {
            mOpenList.Clear();
            mCloseList.Clear();
            mCreatedNodes.Clear();
        }

        private void RetracePath(PathNode endNode, List<Vector3> outPath)
        {
            PathNode currentNode = endNode;
            while (currentNode != null)
            {
                outPath.Add(mBakedMap.GetWorldPosition(currentNode.position.x, currentNode.position.y));
                currentNode = currentNode.parent;
            }

            outPath.Reverse();
        }

        private PathNode GetOrCreateNode(Int2 pos)
        {
            if (mCreatedNodes.TryGetValue(pos, out PathNode node))
            {
                return node;
            }

            PathNode newNode = new PathNode(pos);
            mCreatedNodes.Add(pos, newNode);
            return newNode;
        }

        // ========================================================
        // !!! 新增功能: Line-of-Sight 检查 (Bresenham) !!!
        // ========================================================
        /// <summary>
        /// 检查从start到end的路径是否畅通（任意角度）
        /// </summary>
        private bool HasLineOfSight(Int2 start, Int2 end)
        {
            int x0 = start.x;
            int y0 = start.y;
            int x1 = end.x;
            int y1 = end.y;
            
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            
            int err = dx - dy;
            
            while (true)
            {
                // 1. 检查当前节点 (不检查起点本身)
                if (x0 != start.x || y0 != start.y)
                {
                    if (mBakedMap.IsWall(x0, y0))
                    {
                        return false;
                    }
                }

                // 2. 检查是否到达终点
                if (x0 == x1 && y0 == y1)
                {
                    return true;
                }

                // 3. Bresenham's 步进
                int e2 = 2 * err;
                int prevX = x0;
                int prevY = y0;
                bool movedDiag = false;
                
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }

                // 4. 检查是否为对角线移动
                if (x0 != prevX && y0 != prevY)
                {
                    movedDiag = true;
                }

                // 5. 检查拐角 (角点切割检测)
                if (CHECK_CORNER_CUTTING && movedDiag)
                {
                    // 检查 (prevX, y0) 和 (x0, prevY)
                    if (mBakedMap.IsWall(prevX, y0) || mBakedMap.IsWall(x0, prevY))
                    {
                        return false;
                    }
                }
            }
        }
        
        // (以下函数从 JPSPlus.cs 移植 - 保持不变)
        private int H(Int2 a, Int2 b)
        {
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * MOVE_STRAIGHT_COST;
        }

        private EDirFlags ValidLookUPTable(EDirFlags dir)
        {
            switch (dir)
            {
                case EDirFlags.NORTH:
                    return EDirFlags.EAST | EDirFlags.NORTHEAST | EDirFlags.NORTH | EDirFlags.NORTHWEST | EDirFlags.WEST;
                case EDirFlags.WEST:
                    return EDirFlags.NORTH | EDirFlags.NORTHWEST | EDirFlags.WEST | EDirFlags.SOUTHWEST | EDirFlags.SOUTH;
                case EDirFlags.EAST:
                    return EDirFlags.SOUTH | EDirFlags.SOUTHEAST | EDirFlags.EAST | EDirFlags.NORTHEAST | EDirFlags.NORTH;
                case EDirFlags.SOUTH:
                    return EDirFlags.WEST | EDirFlags.SOUTHWEST | EDirFlags.SOUTH | EDirFlags.SOUTHEAST | EDirFlags.EAST;
                case EDirFlags.NORTHWEST:
                    return EDirFlags.NORTH | EDirFlags.NORTHWEST | EDirFlags.WEST;
                case EDirFlags.NORTHEAST:
                    return EDirFlags.NORTH | EDirFlags.NORTHEAST | EDirFlags.EAST;
                case EDirFlags.SOUTHWEST:
                    return EDirFlags.SOUTH | EDirFlags.SOUTHWEST | EDirFlags.WEST;
                case EDirFlags.SOUTHEAST:
                    return EDirFlags.SOUTH | EDirFlags.SOUTHEAST | EDirFlags.EAST;
                default:
                    return EDirFlags.ALL; // ALL (e.g., for start node)
            }
        }

        private bool IsGoalInExactDirection(Int2 curr, EDirFlags processDir, Int2 goal)
        {
            int dx = goal.x - curr.x;
            int dy = goal.y - curr.y;

            switch (processDir)
            {
                case EDirFlags.NORTH: return dx == 0 && dy > 0; 
                case EDirFlags.SOUTH: return dx == 0 && dy < 0;
                case EDirFlags.WEST: return dx < 0 && dy == 0;
                case EDirFlags.EAST: return dx > 0 && dy == 0;
                case EDirFlags.NORTHWEST: return dx < 0 && dy > 0 && (Mathf.Abs(dx) == Mathf.Abs(dy));
                case EDirFlags.NORTHEAST: return dx > 0 && dy > 0 && (Mathf.Abs(dx) == Mathf.Abs(dy));
                case EDirFlags.SOUTHWEST: return dx < 0 && dy < 0 && (Mathf.Abs(dx) == Mathf.Abs(dy));
                case EDirFlags.SOUTHEAST: return dx > 0 && dy < 0 && (Mathf.Abs(dx) == Mathf.Abs(dy));
                default: return false;
            }
        }

        private bool IsGoalInGeneralDirection(Int2 curr, EDirFlags processDir, Int2 goal)
        {
            int dx = goal.x - curr.x;
            int dy = goal.y - curr.y;

            switch (processDir)
            {
                case EDirFlags.NORTHWEST: return dx < 0 && dy > 0;
                case EDirFlags.NORTHEAST: return dx > 0 && dy > 0;
                case EDirFlags.SOUTHWEST: return dx < 0 && dy < 0;
                case EDirFlags.SOUTHEAST: return dx > 0 && dy < 0;
                default: return IsGoalInExactDirection(curr, processDir, goal);
            }
        }
        
        /// <summary>
        /// 获取上次寻路时探索过的所有节点 (Closed List)
        /// </summary>
        public HashSet<Int2> GetExploredJumpPoints()
        {
            return mCloseList;
        }
    }
}