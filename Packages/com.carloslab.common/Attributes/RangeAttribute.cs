// <copyright file="RangeAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RangeAttribute : Attribute
    {
        public float Min;
        public float Max;

        public RangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}