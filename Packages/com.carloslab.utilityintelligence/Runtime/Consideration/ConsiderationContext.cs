// <copyright file="ConsiderationContext.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct ConsiderationContext : IEquatable<ConsiderationContext>
    {
        public static readonly ConsiderationContext Null = new();

        public ConsiderationContext(Consideration consideration) : this()
        {
            Consideration = consideration;
        }

        public ConsiderationContext(Consideration consideration, in DecisionContext context) : this(consideration)
        {
            Target = context.Target;

            Decision = context.Decision;
            DecisionMaker = context.DecisionMaker;
        }

        #region Consideration

        public readonly Consideration Consideration;
        public string ConsiderationName => Consideration?.Name;

        #endregion

        #region Context

        public readonly UtilityEntity Target;

        public IEntityFacade TargetFacade => Target?.EntityFacade;

        public readonly Decision Decision;

        public string DecisionName => Decision?.Name;

        public readonly DecisionMaker DecisionMaker;

        public string DecisionMakerName => DecisionMaker?.Name;

        #endregion

        #region Result

        public object RawInput;
        public float NormalizedInput;
        public float Score;
        public float FinalScore;
        public ConsiderationStatus CurrentStatus;

        public void SetResult(in ConsiderationResult result)
        {
            RawInput = result.RawInput;
            NormalizedInput = result.NormalizedInput;
            Score = result.Score;
        }

        #endregion

        #region IEquatable

        public bool Equals(ConsiderationContext other)
        {
            return Consideration == other.Consideration
                   && Target == other.Target
                   && Decision == other.Decision
                   && DecisionMaker == other.DecisionMaker;
        }

        public override bool Equals(object obj)
        {
            return obj is ConsiderationContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Consideration, Target, Decision, DecisionMaker);
        }

        public static bool operator ==(ConsiderationContext left, ConsiderationContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConsiderationContext left, ConsiderationContext right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}