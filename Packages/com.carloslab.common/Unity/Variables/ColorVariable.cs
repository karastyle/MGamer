// <copyright file="ColorVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.Common
{
    public class ColorVariable : Variable<Color>
    {
        public static implicit operator ColorVariable(Color value)
        {
            return new ColorVariable { Value = value };
        }
    }
}