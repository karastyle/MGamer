// <copyright file="Int2.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    [StructLayout(LayoutKind.Sequential)]
    [DataContract]
    public partial struct Int2 : IType2<int>, IEquatable<Int2>
    {
        public static readonly Int2 Zero = new(0, 0);
        public static readonly Int2 One = new(1, 1);
        public static readonly Int2 Up = new(0, 1);
        public static readonly Int2 Down = new(0, -1);
        public static readonly Int2 Left = new(-1, 0);
        public static readonly Int2 Right = new(1, 0);

        [DataMember(Name = nameof(X))]
        private int x;

        [DataMember(Name = nameof(Y))]
        private int y;

        public int X
        {
            get => x;
            set => x = value;
        }

        public int Y
        {
            get => y;
            set => y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return MathF.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return x * x + y * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Int2 a, Int2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;

            return MathF.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Int2 a, Int2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;

            return x * x + y * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Float2(Int2 v)
        {
            return new Float2(v.X, v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Int3(Int2 v)
        {
            return new Int3(v.X, v.Y, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator -(Int2 v)
        {
            return new Int2(-v.X, -v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator -(Int2 left, Int2 right)
        {
            return new Int2(left.X - right.X, left.Y - right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator +(Int2 left, Int2 right)
        {
            return new Int2(left.X + right.X, left.Y + right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator *(Int2 left, Int2 right)
        {
            return new Int2(left.X * right.X, left.Y * right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator *(int left, Int2 right)
        {
            return new Int2(left * right.X, left * right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator *(Int2 left, int right)
        {
            return new Int2(left.X * right, left.Y * right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator /(Int2 left, Int2 right)
        {
            return new Int2(left.X / right.X, left.Y / right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator /(Int2 left, int right)
        {
            return new Int2(left.X / right, left.Y / right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Int2 other)
        {
            return x == other.x && y == other.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Int2 other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Int2 left, Int2 right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Int2 left, Int2 right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}