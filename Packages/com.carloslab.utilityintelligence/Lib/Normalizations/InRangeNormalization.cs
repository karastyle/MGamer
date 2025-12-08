// <copyright file="InRangeNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class InRangeNormalization<TValue> : InputNormalization<TValue>
    {
        public VariableReference<TValue> MinValue;
        public VariableReference<TValue> MaxValue;
    }

    [Category("Range")]
    public class InRangeNormalizationFloat : InRangeNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            var diff = MaxValue - MinValue;
            if (diff <= 0.0f) return 0.0f;

            float normalizedInput = (rawInput - MinValue) / (diff);
            return normalizedInput;
        }
    }

    [Category("Range")]
    public class InRangeNormalizationInt : InRangeNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, in InputNormalizationContext context)
        {
            var diff = MaxValue - MinValue;
            if (diff <= 0) return 0.0f;

            float normalizedInput = (float)(rawInput - MinValue) / (diff);
            return normalizedInput;
        }
    }
}