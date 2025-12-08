// <copyright file="BasicNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BasicNormalization<T> : InputNormalization<T>
    {

    }

    [Category("Basic")]
    public class BasicNormalizationBool : BasicNormalization<bool>
    {
        protected override float OnCalculateNormalizedInput(bool rawInput, in InputNormalizationContext context)
        {
            return rawInput ? 1.0f : 0.0f;
        }
    }

    [Category("Basic")]
    public class BasicNormalizationFloat : BasicNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            return rawInput;
        }
    }

    [Category("Basic")]
    public class BasicNormalizationInt : BasicNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, in InputNormalizationContext context)
        {
            return rawInput;
        }
    }
}