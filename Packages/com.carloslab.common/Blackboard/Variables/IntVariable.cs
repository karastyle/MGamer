// <copyright file="IntVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.Common
{
    [Category("Basic")]
    public class IntVariable : Variable<int>
    {
        public static implicit operator IntVariable(int value)
        {
            return new IntVariable { Value = value };
        }
    }
}