// <copyright file="IsInChargeRadiusNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class IsInChargeRadiusNormalization : InputNormalization<float>
    {
        public float ChargeRadius = 2;

        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = rawInput >= 0 && rawInput <= GetChargeRadius(in context) ? 1.0f : 0.0f;
            return normalizedInput;
        }

        private float GetChargeRadius(in InputNormalizationContext context)
        {
            if (context is { TargetFacade: ChargeStation chargeStation })
                return chargeStation.ChargeRadius;
            return ChargeRadius;
        }
    }
}