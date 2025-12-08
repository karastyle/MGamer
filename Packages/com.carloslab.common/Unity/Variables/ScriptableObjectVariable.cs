// <copyright file="ScriptableObjectVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("ScriptableObject")]
    public class ScriptableObjectVariable : Variable<ScriptableObject>
    {
        public static implicit operator ScriptableObjectVariable(ScriptableObject value)
        {
            return new ScriptableObjectVariable { Value = value };
        }
    }
}