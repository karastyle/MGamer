// <copyright file="Vector3IntVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("Vector3")]
    public class Vector3IntVariable : Variable<Vector3Int>
    {
        public static implicit operator Vector3IntVariable(Vector3Int value)
        {
            return new Vector3IntVariable { Value = value };
        }
    }
}