// <copyright file="InputNormalizationContext.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct InputNormalizationContext : IEquatable<InputNormalizationContext>
    {
        public static readonly InputNormalizationContext Null = new();

        #region InputNormalization

        public readonly InputNormalization InputNormalization;
        public string InputNormalizationName => InputNormalization?.Name;

        #endregion

        #region Context

        public readonly UtilityEntity Target;
        public IEntityFacade TargetFacade => Target?.EntityFacade;

        public readonly Consideration Consideration;
        public string ConsiderationName => Consideration?.Name;

        public readonly Decision Decision;

        public string DecisionName => Decision?.Name;

        public readonly DecisionMaker DecisionMaker;

        public string DecisionMakerName => DecisionMaker?.Name;

        #endregion

        public InputNormalizationContext(InputNormalization inputNormalization) : this()
        {
            InputNormalization = inputNormalization;
        }

        public InputNormalizationContext(InputNormalization inputNormalization, in ConsiderationContext context) : this(inputNormalization)
        {
            Target = context.Target;

            Consideration = context.Consideration;
            Decision = context.Decision;
            DecisionMaker = context.DecisionMaker;
        }

        public bool Equals(InputNormalizationContext other)
        {
            return InputNormalization == other.InputNormalization
                   && Target == other.Target
                   && Consideration == other.Consideration
                   && Decision == other.Decision
                   && DecisionMaker == other.DecisionMaker;

        }

        // public bool EqualsValue(InputNormalizationContext other)
        // {
        //     return Equals(other)
        //            && Math.Abs(NormalizedInput - other.NormalizedInput) < MathUtils.Epsilon;
        // }

        public override bool Equals(object obj)
        {
            return obj is InputNormalizationContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(InputNormalization, Target);
        }

        public static bool operator ==(InputNormalizationContext left, InputNormalizationContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputNormalizationContext left, InputNormalizationContext right)
        {
            return !left.Equals(right);
        }
    }
}