// <copyright file="TransformVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("Transform")]
    public class TransformVariable : Variable<Transform>
    {
        public static implicit operator TransformVariable(Transform value)
        {
            return new TransformVariable { Value = value };
        }
    }
}