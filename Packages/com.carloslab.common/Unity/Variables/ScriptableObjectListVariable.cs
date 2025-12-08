// <copyright file="ScriptableObjectListVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("ScriptableObject")]
    public class ScriptableObjectListVariable : Variable<List<ScriptableObject>>
    {
        public static implicit operator ScriptableObjectListVariable(List<ScriptableObject> value)
        {
            return new ScriptableObjectListVariable { Value = value };
        }
    }
}