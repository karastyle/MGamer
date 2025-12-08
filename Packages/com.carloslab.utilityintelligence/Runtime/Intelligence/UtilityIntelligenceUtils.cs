// <copyright file="UtilityIntelligenceUtils.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public static class UtilityIntelligenceUtils
    {
        public static float CalculateGeometricMean(float[] considerationScores)
        {
            if (considerationScores.Length == 0)
            {
                throw new ArgumentException("Array of scores must not be empty.");
            }
            float product = 1.0f;

            foreach (float considerationScore in considerationScores)
            {
                product *= considerationScore;
            }

            float geometricMean = MathF.Pow(product, 1.0f / considerationScores.Length);

            return geometricMean;
        }
    }
}
