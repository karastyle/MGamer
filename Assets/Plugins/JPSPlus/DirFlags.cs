// 放置在 Assets/Scripts/Helpers/DirFlags.cs
using System;

namespace JPSPlus
{
    [Flags]
    public enum EDirFlags
    {
        NONE = 0,
        NORTH = 1,
        SOUTH = 2,
        WEST = 4,
        EAST = 8,
        NORTHWEST = 16,
        NORTHEAST = 32,
        SOUTHWEST = 64,
        SOUTHEAST = 128,
        ALL = NORTH | SOUTH | WEST | EAST | NORTHWEST | NORTHEAST | SOUTHWEST | SOUTHEAST,
        STRAIGHT = NORTH | SOUTH | WEST | EAST,
        DIAGONAL = NORTHWEST | NORTHEAST | SOUTHWEST | SOUTHEAST,
    }

    public static class DirFlags
    {
        // 将 8 个方向（N, S, W, E, NW, NE, SW, SE）映射到 0-7 的数组索引
        public static int ToArrayIndex(EDirFlags dir)
        {
            switch (dir)
            {
                case EDirFlags.NORTH: return 0;
                case EDirFlags.SOUTH: return 1;
                case EDirFlags.WEST: return 2;
                case EDirFlags.EAST: return 3;
                case EDirFlags.NORTHWEST: return 4;
                case EDirFlags.NORTHEAST: return 5;
                case EDirFlags.SOUTHWEST: return 6;
                case EDirFlags.SOUTHEAST: return 7;
                default: throw new ArgumentException("Invalid single direction flag");
            }
        }

        public static EDirFlags FromArrayIndex(int index)
        {
            switch (index)
            {
                case 0: return EDirFlags.NORTH;
                case 1: return EDirFlags.SOUTH;
                case 2: return EDirFlags.WEST;
                case 3: return EDirFlags.EAST;
                case 4: return EDirFlags.NORTHWEST;
                case 5: return EDirFlags.NORTHEAST;
                case 6: return EDirFlags.SOUTHWEST;
                case 7: return EDirFlags.SOUTHEAST;
                default: throw new ArgumentException("Invalid array index");
            }
        }

        public static Int2 ToPos(EDirFlags dir)
        {
            switch (dir)
            {
                case EDirFlags.NORTH: return new Int2(0, 1);
                case EDirFlags.SOUTH: return new Int2(0, -1);
                case EDirFlags.WEST: return new Int2(-1, 0);
                case EDirFlags.EAST: return new Int2(1, 0);
                case EDirFlags.NORTHWEST: return new Int2(-1, 1);
                case EDirFlags.NORTHEAST: return new Int2(1, 1);
                case EDirFlags.SOUTHWEST: return new Int2(-1, -1);
                case EDirFlags.SOUTHEAST: return new Int2(1, -1);
                default: throw new ArgumentException("Invalid single direction flag");
            }
        }

        public static bool IsDiagonal(EDirFlags dir)
        {
            return (dir & EDirFlags.DIAGONAL) != EDirFlags.NONE;
        }
    }
}