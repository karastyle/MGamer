// <copyright file="Float2.cs" company="CarlosLab">
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
    public partial struct Float2 : IType2<float>, IEquatable<Float2>
    {
        public static readonly Float2 Zero = new(0, 0);
        public static readonly Float2 One = new(1, 1);
        public static readonly Float2 Up = new(0, 1);
        public static readonly Float2 Down = new(0, -1);
        public static readonly Float2 Left = new(-1, 0);
        public static readonly Float2 Right = new(1, 0);
        public static readonly Float2 PositiveInfinity = new(float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Float2 NegativeInfinity = new(float.NegativeInfinity, float.NegativeInfinity);

        [DataMember(Name = nameof(X))]
        private float x;

        [DataMember(Name = nameof(Y))]
        private float y;

        public float X
        {
            get => x;
            set => x = value;
        }

        public float Y
        {
            get => y;
            set => y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Float2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Float2 a, Float2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;

            return MathF.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Float2 a, Float2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;

            return x * x + y * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Int2(Float2 v)
        {
            return new Int2((int)v.X, (int)v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Float3(Float2 v)
        {
            return new Float3(v.X, v.Y, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator -(Float2 v)
        {
            return new Float2(-v.X, -v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator -(Float2 left, Float2 right)
        {
            return new Float2(left.X - right.X, left.Y - right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator +(Float2 left, Float2 right)
        {
            return new Float2(left.X + right.X, left.Y + right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator *(Float2 left, Float2 right)
        {
            return new Float2(left.X * right.X, left.Y * right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator *(int scale, Float2 v)
        {
            return new Float2(scale * v.X, scale * v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator *(Float2 v, int scale)
        {
            return new Float2(v.X * scale, v.Y * scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float2 operator /(Float2 v, int scale)
        {
            return new Float2(v.X / scale, v.Y / scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Float2 other)
        {
            float diffX = X - other.X;
            float diffY = Y - other.Y;
            return MathF.Abs(diffX) < MathUtils.Epsilon
                   && MathF.Abs(diffY) < MathUtils.Epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Float2 other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Float2 left, Float2 right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Float2 left, Float2 right)
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