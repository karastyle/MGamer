// <copyright file="Int3.cs" company="CarlosLab">
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
    public partial struct Int3 : IEquatable<Int3>
    {
        public static readonly Int3 Zero = new(0, 0, 0);
        public static readonly Int3 One = new(1, 1, 1);
        public static readonly Int3 Up = new(0, 1, 0);
        public static readonly Int3 Down = new(0, -1, 0);
        public static readonly Int3 Left = new(-1, 0, 0);
        public static readonly Int3 Right = new(1, 0, 0);
        public static readonly Int3 Forward = new(0, 0, 1);
        public static readonly Int3 Back = new(0, 0, 1);

        [DataMember(Name = nameof(X))]
        private int x;

        [DataMember(Name = nameof(Y))]
        private int y;

        [DataMember(Name = nameof(Z))]
        private int z;

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

        public int Z
        {
            get => z;
            set => z = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Float3(Int3 v)
        {
            return new Float3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Int2(Int3 v)
        {
            return new Int2(v.X, v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int3 operator +(Int3 left, Int3 right)
        {
            return new Int3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int3 operator -(Int3 left, Int3 right)
        {
            return new Int3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int3 operator *(Int3 left, int right)
        {
            return new Int3(left.X * right, left.Y * right, left.Z * right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int3 operator *(int left, Int3 right)
        {
            return new Int3(left * right.X, left * right.Y, left * right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int3 operator /(Int3 left, int right)
        {
            return new Int3(left.X / right, left.Y / right, left.Z / right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Int3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Int3 other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Int3 left, Int3 right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Int3 left, Int3 right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}