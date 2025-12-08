// <copyright file="Vector2IntVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("Vector2")]
    public class Vector2IntVariable : Variable<Vector2Int>
    {
        public static implicit operator Vector2IntVariable(Vector2Int value)
        {
            return new Vector2IntVariable { Value = value };
        }
    }
}