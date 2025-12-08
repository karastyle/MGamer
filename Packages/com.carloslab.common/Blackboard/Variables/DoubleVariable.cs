// <copyright file="DoubleVariable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.Common
{
    [Category("Basic")]
    public class DoubleVariable : Variable<double>
    {
        public static implicit operator DoubleVariable(double value)
        {
            return new DoubleVariable { Value = value };
        }
    }
}