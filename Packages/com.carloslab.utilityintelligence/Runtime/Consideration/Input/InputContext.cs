// <copyright file="InputContext.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct InputContext : IEquatable<InputContext>
    {
        public static readonly InputContext Null = new();

        #region Input

        public readonly Input Input;

        public string InputName => Input?.Name;

        #endregion

        #region Context

        public readonly UtilityEntity Target;
        public IEntityFacade TargetFacade => Target?.EntityFacade;

        public readonly InputNormalization InputNormalization;
        public string InputNormalizationName => InputNormalization?.Name;

        public readonly Consideration Consideration;
        public string ConsiderationName => Consideration?.Name;

        public readonly Decision Decision;

        public string DecisionName => Decision?.Name;

        public readonly DecisionMaker DecisionMaker;

        public string DecisionMakerName => DecisionMaker?.Name;

        #endregion

        public InputContext(Input input) : this()
        {
            Input = input;
        }

        public InputContext(Input input, in InputNormalizationContext context) : this(input)
        {
            Target = context.Target;

            InputNormalization = context.InputNormalization;
            Consideration = context.Consideration;
            Decision = context.Decision;
            DecisionMaker = context.DecisionMaker;
        }

        public bool Equals(InputContext other)
        {
            return Input == other.Input
                   && Target == other.Target
                   && InputNormalization == other.InputNormalization
                   && Consideration == other.Consideration
                   && Decision == other.Decision
                   && DecisionMaker == other.DecisionMaker;
        }

        // public bool EqualsValue(InputContext other)
        // {
        //     return Equals(other)
        //     && RawInput == other.RawInput
        //     && Math.Abs(NormalizedInput - other.NormalizedInput) < MathUtils.Epsilon;
        // }

        public override bool Equals(object obj)
        {
            return obj is InputContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Input, Target);
        }

        public static bool operator ==(InputContext left, InputContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputContext left, InputContext right)
        {
            return !left.Equals(right);
        }
    }
}