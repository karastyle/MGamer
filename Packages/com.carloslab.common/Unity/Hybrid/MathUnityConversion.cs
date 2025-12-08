// <copyright file="MathUnityConversion.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

#if UNITY_5_3_OR_NEWER

using System.Runtime.CompilerServices;
using UnityEngine;

namespace CarlosLab.Common
{
    public partial struct Float3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3(Float3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static implicit operator Float3(Vector3 v)
        {
            return new Float3(v.x, v.y, v.z);
        }
    }

    public partial struct Float2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Float2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static implicit operator Float2(Vector2 v)
        {
            return new Float2(v.x, v.y);
        }
    }

    public partial struct Int3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3Int(Int3 v)
        {
            return new Vector3Int(v.X, v.Y, v.Z);
        }

        public static implicit operator Int3(Vector3Int v)
        {
            return new Int3(v.x, v.y, v.z);
        }
    }

    public partial struct Int2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2Int(Int2 v)
        {
            return new Vector2Int(v.X, v.Y);
        }

        public static implicit operator Int2(Vector2Int v)
        {
            return new Int2(v.x, v.y);
        }
    }
}
#endif