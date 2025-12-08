// <copyright file="StringVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.Common
{
    [Category("Basic")]
    public class StringVariable : Variable<string>
    {
        public static implicit operator StringVariable(string value)
        {
            return new StringVariable { Value = value };
        }
    }
}