// <copyright file="DecisionResult.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct DecisionResult : IEquatable<DecisionResult>
    {
        public static readonly DecisionResult Null = new();

        public DecisionResult(Decision decision) : this()
        {
            Decision = decision;
        }

        public readonly Decision Decision;

        public bool Discarded;
        public float Score;

        #region IEquatable

        public bool Equals(DecisionResult other)
        {
            return Decision == other.Decision;
        }

        public override bool Equals(object obj)
        {
            return obj is DecisionResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Decision.GetHashCode();
        }

        public static bool operator ==(DecisionResult left, DecisionResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DecisionResult left, DecisionResult right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}