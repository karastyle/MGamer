// <copyright file="Vector2Variable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("Vector2")]
    public class Vector2Variable : Variable<Vector2>
    {
        public static implicit operator Vector2Variable(Vector2 value)
        {
            return new Vector2Variable { Value = value };
        }
    }
}