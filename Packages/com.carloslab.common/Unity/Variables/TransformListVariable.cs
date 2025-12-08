// <copyright file="TransformListVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("Transform")]
    public class TransformListVariable : Variable<List<Transform>>
    {
        public static implicit operator TransformListVariable(List<Transform> value)
        {
            return new TransformListVariable { Value = value };
        }
    }
}
