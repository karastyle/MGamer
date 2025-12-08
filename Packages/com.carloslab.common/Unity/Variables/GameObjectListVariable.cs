// <copyright file="GameObjectListVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.Common
{
    [Category("GameObject")]
    public class GameObjectListVariable : Variable<List<GameObject>>
    {
        public static implicit operator GameObjectListVariable(List<GameObject> value)
        {
            return new GameObjectListVariable { Value = value };
        }
    }
}
