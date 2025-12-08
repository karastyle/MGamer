// <copyright file="BoolVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.Common
{
    [Category("Basic")]
    public class BoolVariable : Variable<bool>
    {
        public static implicit operator BoolVariable(bool value)
        {
            return new BoolVariable { Value = value };
        }
    }
}