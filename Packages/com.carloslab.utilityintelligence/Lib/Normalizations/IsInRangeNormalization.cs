// <copyright file="IsInRangeNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence
{
    [Category("Range")]
    public class IsInRangeNormalizationFloat : InRangeNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = rawInput >= MinValue && rawInput <= MaxValue ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }

    [Category("Range")]
    public class IsInRangeNormalizationInt : InRangeNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = rawInput >= MinValue && rawInput <= MaxValue ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }
}