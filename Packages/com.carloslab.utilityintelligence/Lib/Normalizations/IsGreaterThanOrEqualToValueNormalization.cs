// <copyright file="IsGreaterThanOrEqualToValueNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class IsGreaterThanOrEqualToValueNormalization<T> : InputNormalization<T>
    {
        public VariableReference<T> Value;
    }

    [Category("Comparison")]
    public class IsGreaterThanOrEqualToValueNormalizationFloat : IsGreaterThanOrEqualToValueNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = rawInput >= Value ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }

    [Category("Comparison")]
    public class IsGreaterThanOrEqualToValueNormalizationInt : IsGreaterThanOrEqualToValueNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = rawInput >= Value ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }
}