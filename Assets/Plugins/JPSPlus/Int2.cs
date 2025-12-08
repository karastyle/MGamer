// 放置在 Assets/Scripts/Helpers/Int2.cs
using System;
using UnityEngine;

namespace JPSPlus
{
    [System.Serializable]
    public struct Int2 : IEquatable<Int2>
    {
        public int x;
        public int y;

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Int2 operator +(Int2 a, Int2 b)
        {
            return new Int2(a.x + b.x, a.y + b.y);
        }

        public static Int2 operator *(Int2 a, int b)
        {
            return new Int2(a.x * b, a.y * b);
        }

        public static Vector3 ToVector3(Int2 a)
        {
            return new Vector3(a.x, 0, a.y);
        }

        public bool Equals(Int2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Int2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        public static bool operator ==(Int2 a, Int2 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Int2 a, Int2 b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}