// <copyright file="GameObjectVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("GameObject")]
    public class GameObjectVariable : Variable<GameObject>
    {
        public static implicit operator GameObjectVariable(GameObject value)
        {
            return new GameObjectVariable { Value = value };
        }
    }
}
