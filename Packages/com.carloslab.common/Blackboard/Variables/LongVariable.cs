// <copyright file="LongVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.Common
{
    [Category("Basic")]
    public class LongVariable : Variable<long>
    {
        public static implicit operator LongVariable(long value)
        {
            return new LongVariable { Value = value };
        }
    }
}