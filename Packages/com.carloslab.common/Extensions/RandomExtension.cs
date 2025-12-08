// <copyright file="RandomExtension.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Extensions
{
    public static class RandomExtension
    {
        public static double NextDouble(this Random rand, double minValue, double maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), minValue,
                    $"'{nameof(minValue)}' must be smaller than or equal to {nameof(maxValue)}.");
            }

            return (maxValue - minValue) * rand.NextDouble() + minValue;
        }

        public static float NextFloat(this Random rand, float minValue, float maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), minValue,
                    $"'{nameof(minValue)}' must be smaller than or equal to {nameof(maxValue)}.");
            }

            return (float)rand.NextDouble(minValue, maxValue);
        }
    }
}