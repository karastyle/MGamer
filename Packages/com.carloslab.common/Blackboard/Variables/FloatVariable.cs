// <copyright file="FloatVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.Common
{
    [Category("Basic")]
    public class FloatVariable : Variable<float>
    {
        public static implicit operator FloatVariable(float value)
        {
            return new FloatVariable { Value = value };
        }
    }
}