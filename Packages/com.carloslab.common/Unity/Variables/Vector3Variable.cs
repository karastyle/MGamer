// <copyright file="Vector3Variable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("Vector3")]
    public class Vector3Variable : Variable<Vector3>
    {
        public static implicit operator Vector3Variable(Vector3 value)
        {
            return new Vector3Variable { Value = value };
        }
    }
}