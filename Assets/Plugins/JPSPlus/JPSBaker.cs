// 放置在 Assets/Scripts/JPSBaker.cs

using UnityEngine;

namespace JPSPlus
{
    // 放置在 Assets/Scripts/JPSBaker.cs
    using UnityEngine;

    /// <summary>
    /// JPS+ 烘焙器
    /// (已修正为 Y-Up 坐标系, 匹配 JPSGrid.cs)
    /// (已修正 MarkPrimary 的 C# 语法错误)
    /// </summary>
    public static class JPSBaker
    {
        // 烘焙时使用的临时节点数据
        private class BakerNode
        {
            public int[] jumpDistances = new int[8];
            public EDirFlags jumpDirFlags = EDirFlags.NONE;
            public bool IsJumpable(EDirFlags dir) => (jumpDirFlags & dir) == dir;
            public void SetDistance(EDirFlags dir, int distance) => jumpDistances[DirFlags.ToArrayIndex(dir)] = distance;
            public int GetDistance(EDirFlags dir) => jumpDistances[DirFlags.ToArrayIndex(dir)];
        }

        // 烘焙期间的临时状态
        private static int mWidth;
        private static int mHeight;
        private static bool[,] mWalls;
        private static BakerNode[,] mNodes; // [y, x]

        // 辅助函数：检查是否可通行
        private static bool IsWalkable(int x, int y)
        {
            if (x < 0 || x >= mWidth || y < 0 || y >= mHeight) return false;
            return !mWalls[y, x];
        }

        // 辅助函数：获取临时节点（如果可通行）
        private static BakerNode GetNode(int x, int y)
        {
            if (x < 0 || x >= mWidth || y < 0 || y >= mHeight) return null;
            return mNodes[y, x];
        }

        /// <summary>
        /// 烘焙JPS+数据
        /// </summary>
        public static void Bake(JPSBakedData data)
        {
            mWidth = data.gridWidth;
            mHeight = data.gridHeight;

            // 1. 从烘焙数据中解压墙体信息
            mWalls = new bool[mHeight, mWidth];
            for (int y = 0; y < mHeight; y++)
            for (int x = 0; x < mWidth; x++)
                mWalls[y, x] = data.IsWall(x, y);

            // 2. 初始化烘焙节点（只为可通行格子创建）
            mNodes = new BakerNode[mHeight, mWidth];
            for (int y = 0; y < mHeight; y++)
            for (int x = 0; x < mWidth; x++)
                if (IsWalkable(x, y))
                    mNodes[y, x] = new BakerNode();

            // 3. 标记主要跳点 (使用“墙角”逻辑)
            MarkPrimary();

            // 4. 标记直线跳点 (使用“距离传播”逻辑)
            MarkStraight();

            // 5. 标记对角线跳点 (使用“传播优先”逻辑)
            MarkDiagonal();

            // 6. 将结果复制回 JPSBakedData
            for (int y = 0; y < mHeight; y++)
            for (int x = 0; x < mWidth; x++)
                if (IsWalkable(x, y))
                {
                    data.SetJumpFlags(x, y, mNodes[y, x].jumpDirFlags);
                    for (int dir = 0; dir < 8; dir++)
                        data.SetDistance(x, y, dir, mNodes[y, x].jumpDistances[dir]);
                }

            // 7. 清理内存
            mWalls = null;
            mNodes = null;
        }

        // ======================================================
        //   !!!! MarkPrimary (已修正C#语法) !!!!
        // ======================================================
        /// <summary>
        /// 标记主要跳点 (强制邻居) - Y轴向上
        /// (已修正 C# 语法错误)
        /// </summary>
        private static void MarkPrimary()
        {
            for (int y = 0; y < mHeight; ++y)
            {
                for (int x = 0; x < mWidth; ++x)
                {
                    // 遍历所有格子，如果当前格(x,y)是墙壁，则检查它周围
                    if (IsWalkable(x, y))
                    {
                        continue;
                    }

                    // (x, y) 是一个墙壁

                    // 1. 检查 NORTHEAST (来自 SW)
                    if (IsWalkable(x, y + 1) && IsWalkable(x + 1, y))
                    {
                        var node = GetNode(x + 1, y + 1); // 标记 NE 格子
                        if (node != null)
                            node.jumpDirFlags |= EDirFlags.SOUTH | EDirFlags.WEST;
                    }

                    // 2. 检查 SOUTHEAST (来自 NW)
                    if (IsWalkable(x, y - 1) && IsWalkable(x + 1, y))
                    {
                        var node = GetNode(x + 1, y - 1); // 标记 SE 格子
                        if (node != null)
                            node.jumpDirFlags |= EDirFlags.NORTH | EDirFlags.WEST;
                    }

                    // 3. 检查 NORTHWEST (来自 SE)
                    if (IsWalkable(x, y + 1) && IsWalkable(x - 1, y))
                    {
                        var node = GetNode(x - 1, y + 1); // 标记 NW 格子
                        if (node != null)
                            node.jumpDirFlags |= EDirFlags.SOUTH | EDirFlags.EAST;
                    }

                    // 4. 检查 SOUTHWEST (来自 NE)
                    if (IsWalkable(x, y - 1) && IsWalkable(x - 1, y))
                    {
                        var node = GetNode(x - 1, y - 1); // 标记 SW 格子
                        if (node != null)
                            node.jumpDirFlags |= EDirFlags.NORTH | EDirFlags.EAST;
                    }
                }
            }
        }

        /// <summary>
        /// 标记直线跳点 - Y轴向上 (已修正)
        /// </summary>
        private static void MarkStraight()
        {
            // WEST (L-R)
            for (int y = 0; y < mHeight; ++y)
            {
                int d = -1;
                bool jp = false;
                for (int x = 0; x < mWidth; ++x)
                {
                    var b = GetNode(x, y);
                    if (b == null)
                    {
                        d = -1;
                        jp = false;
                        continue;
                    }

                    d++;
                    if (jp) b.SetDistance(EDirFlags.WEST, d);
                    else b.SetDistance(EDirFlags.WEST, -d);
                    if (b.IsJumpable(EDirFlags.EAST))
                    {
                        d = 0;
                        jp = true;
                    }
                }
            }

            // EAST (R-L)
            for (int y = 0; y < mHeight; ++y)
            {
                int d = -1;
                bool jp = false;
                for (int x = mWidth - 1; x >= 0; --x)
                {
                    var b = GetNode(x, y);
                    if (b == null)
                    {
                        d = -1;
                        jp = false;
                        continue;
                    }

                    d++;
                    if (jp) b.SetDistance(EDirFlags.EAST, d);
                    else b.SetDistance(EDirFlags.EAST, -d);
                    if (b.IsJumpable(EDirFlags.WEST))
                    {
                        d = 0;
                        jp = true;
                    }
                }
            }

            // SOUTH (B-T)
            for (int x = 0; x < mWidth; ++x)
            {
                int d = -1;
                bool jp = false;
                for (int y = 0; y < mHeight; ++y)
                {
                    var b = GetNode(x, y);
                    if (b == null)
                    {
                        d = -1;
                        jp = false;
                        continue;
                    }

                    d++;
                    if (jp) b.SetDistance(EDirFlags.SOUTH, d);
                    else b.SetDistance(EDirFlags.SOUTH, -d);
                    if (b.IsJumpable(EDirFlags.NORTH))
                    {
                        d = 0;
                        jp = true;
                    }
                }
            }

            // NORTH (T-B)
            for (int x = 0; x < mWidth; ++x)
            {
                int d = -1;
                bool jp = false;
                for (int y = mHeight - 1; y >= 0; --y)
                {
                    var b = GetNode(x, y);
                    if (b == null)
                    {
                        d = -1;
                        jp = false;
                        continue;
                    }

                    d++;
                    if (jp) b.SetDistance(EDirFlags.NORTH, d);
                    else b.SetDistance(EDirFlags.NORTH, -d);
                    if (b.IsJumpable(EDirFlags.SOUTH))
                    {
                        d = 0;
                        jp = true;
                    }
                }
            }
        }

        /// <summary>
        /// 标记对角线跳点 - Y轴向上
        /// (已修正逻辑优先级和正确的扫描顺序)
        /// </summary>
        private static void MarkDiagonal()
        {
            // NORTHWEST (x-, y+)
            // prev = (x-1, y+1) -> 要求 prev 已被处理，所以先从 y = top -> bottom (mHeight-1 -> 0)，x 从 left -> right (0 -> mWidth-1)
            for (int y = mHeight - 1; y >= 0; --y)
            {
                for (int x = 0; x < mWidth; ++x)
                {
                    var block = GetNode(x, y);
                    if (block == null) continue;
                    if (x == 0 || y == mHeight - 1 || !IsWalkable(x - 1, y) || !IsWalkable(x, y + 1) || !IsWalkable(x - 1, y + 1))
                    {
                        block.SetDistance(EDirFlags.NORTHWEST, 0);
                        continue;
                    }

                    var prevBlock = GetNode(x - 1, y + 1); // (NW of current)
                    int d = prevBlock.GetDistance(EDirFlags.NORTHWEST);
                    if (d > 0)
                    {
                        block.SetDistance(EDirFlags.NORTHWEST, d + 1);
                        continue;
                    }

                    if (prevBlock.GetDistance(EDirFlags.NORTH) > 0 || prevBlock.GetDistance(EDirFlags.WEST) > 0)
                    {
                        block.SetDistance(EDirFlags.NORTHWEST, 1);
                        continue;
                    }

                    block.SetDistance(EDirFlags.NORTHWEST, 0);
                }
            }

            // NORTHEAST (x+, y+)
            // prev = (x+1, y+1) -> 要求 prev 已被处理，所以遍历 y 从 top -> bottom, x 从 right -> left
            for (int y = mHeight - 1; y >= 0; --y)
            {
                for (int x = mWidth - 1; x >= 0; --x)
                {
                    var block = GetNode(x, y);
                    if (block == null) continue;
                    if (x == mWidth - 1 || y == mHeight - 1 || !IsWalkable(x + 1, y) || !IsWalkable(x, y + 1) || !IsWalkable(x + 1, y + 1))
                    {
                        block.SetDistance(EDirFlags.NORTHEAST, 0);
                        continue;
                    }

                    var prevBlock = GetNode(x + 1, y + 1); // (NE of current)
                    int d = prevBlock.GetDistance(EDirFlags.NORTHEAST);
                    if (d > 0)
                    {
                        block.SetDistance(EDirFlags.NORTHEAST, d + 1);
                        continue;
                    }

                    if (prevBlock.GetDistance(EDirFlags.NORTH) > 0 || prevBlock.GetDistance(EDirFlags.EAST) > 0)
                    {
                        block.SetDistance(EDirFlags.NORTHEAST, 1);
                        continue;
                    }

                    block.SetDistance(EDirFlags.NORTHEAST, 0);
                }
            }

            // SOUTHWEST (x-, y-)
            // prev = (x-1, y-1) -> 要求 prev 已被处理，所以遍历 y 从 bottom -> top (0 -> mHeight-1), x 从 left -> right
            for (int y = 0; y < mHeight; ++y)
            {
                for (int x = 0; x < mWidth; ++x)
                {
                    var block = GetNode(x, y);
                    if (block == null) continue;
                    if (x == 0 || y == 0 || !IsWalkable(x - 1, y) || !IsWalkable(x, y - 1) || !IsWalkable(x - 1, y - 1))
                    {
                        block.SetDistance(EDirFlags.SOUTHWEST, 0);
                        continue;
                    }

                    var prevBlock = GetNode(x - 1, y - 1); // (SW of current)
                    int d = prevBlock.GetDistance(EDirFlags.SOUTHWEST);
                    if (d > 0)
                    {
                        block.SetDistance(EDirFlags.SOUTHWEST, d + 1);
                        continue;
                    }

                    if (prevBlock.GetDistance(EDirFlags.SOUTH) > 0 || prevBlock.GetDistance(EDirFlags.WEST) > 0)
                    {
                        block.SetDistance(EDirFlags.SOUTHWEST, 1);
                        continue;
                    }

                    block.SetDistance(EDirFlags.SOUTHWEST, 0);
                }
            }

            // SOUTHEAST (x+, y-)
            // prev = (x+1, y-1) -> 要求 prev 已被处理，所以遍历 y 从 bottom -> top, x 从 right -> left
            for (int y = 0; y < mHeight; ++y)
            {
                for (int x = mWidth - 1; x >= 0; --x)
                {
                    var block = GetNode(x, y);
                    if (block == null) continue;
                    if (x == mWidth - 1 || y == 0 || !IsWalkable(x + 1, y) || !IsWalkable(x, y - 1) || !IsWalkable(x + 1, y - 1))
                    {
                        block.SetDistance(EDirFlags.SOUTHEAST, 0);
                        continue;
                    }

                    var prevBlock = GetNode(x + 1, y - 1); // (SE of current)
                    int d = prevBlock.GetDistance(EDirFlags.SOUTHEAST);
                    if (d > 0)
                    {
                        block.SetDistance(EDirFlags.SOUTHEAST, d + 1);
                        continue;
                    }

                    if (prevBlock.GetDistance(EDirFlags.SOUTH) > 0 || prevBlock.GetDistance(EDirFlags.EAST) > 0)
                    {
                        block.SetDistance(EDirFlags.SOUTHEAST, 1);
                        continue;
                    }

                    block.SetDistance(EDirFlags.SOUTHEAST, 0);
                }
            }
        }
    }
}