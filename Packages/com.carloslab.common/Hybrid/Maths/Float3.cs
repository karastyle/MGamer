// <copyright file="Float3.cs" company="CarlosLab">
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
    public partial struct Float3 : IType3<float>, IEquatable<Float3>
    {
        public static readonly Float3 Zero = new(0, 0, 0);
        public static readonly Float3 One = new(1, 1, 1);
        public static readonly Float3 Up = new(0, 1, 0);
        public static readonly Float3 Down = new(0, -1, 0);
        public static readonly Float3 Left = new(-1, 0, 0);
        public static readonly Float3 Right = new(1, 0, 0);
        public static readonly Float3 Forward = new(0, 0, 1);
        public static readonly Float3 Back = new(0, 0, 1);

        public static readonly Float3 PositiveInfinity =
            new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        public static readonly Float3 NegativeInfinity =
            new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        [DataMember(Name = nameof(X))]
        private float x;

        [DataMember(Name = nameof(Y))]
        private float y;

        [DataMember(Name = nameof(Z))]
        private float z;

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

        public float Z
        {
            get => z;
            set => z = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return MathF.Sqrt(x * x + y * y + z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return x * x + y * y + z * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float length = Length();
            if (length > MathUtils.Epsilon)
                this /= length;
            else
                this = Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Float3 a, Float3 b)
        {
            float diffX = a.X - b.X;
            float diffY = a.Y - b.Y;
            float diffZ = a.Z - b.Z;

            return MathF.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Float3 a, Float3 b)
        {
            float diffX = a.X - b.X;
            float diffY = a.Y - b.Y;
            float diffZ = a.Z - b.Z;
            return diffX * diffX + diffY * diffY + diffZ * diffZ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Int3(Float3 v)
        {
            return new Int3((int)v.X, (int)v.Y, (int)v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Float2(Float3 v)
        {
            return new Float2(v.X, v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator +(Float3 left, Float3 right)
        {
            return new Float3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator -(Float3 left, Float3 right)
        {
            return new Float3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator -(Float3 v)
        {
            return new Float3(-v.X, -v.Y, -v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator *(Float3 left, Float3 right)
        {
            return new Float3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator *(Float3 left, float right)
        {
            return new Float3(left.X * right, left.Y * right, left.Z * right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator *(float left, Float3 right)
        {
            return new Float3(left * right.X, left * right.Y, left * right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator /(Float3 left, Float3 right)
        {
            return new Float3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator /(Float3 left, float right)
        {
            return new Float3(left.X / right, left.Y / right, left.Z / right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float3 operator /(float left, Float3 right)
        {
            return new Float3(left / right.X, left / right.Y, left / right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Float3 other)
        {
            float diffX = X - other.X;
            float diffY = Y - other.Y;
            float diffZ = Z - other.Z;
            return MathF.Abs(diffX) < MathUtils.Epsilon
                   && MathF.Abs(diffY) < MathUtils.Epsilon
                   && MathF.Abs(diffZ) < MathUtils.Epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Float3 other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Float3 left, Float3 right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Float3 left, Float3 right)
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