// <copyright file="ConsiderationResult.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct ConsiderationResult : IEquatable<ConsiderationResult>
    {
        public static readonly ConsiderationResult Null = new();

        public ConsiderationResult(Consideration consideration) : this()
        {
            Consideration = consideration;
        }

        public readonly Consideration Consideration;

        public object RawInput;
        public float NormalizedInput;
        public float Score;

        #region IEquatable

        public bool Equals(ConsiderationResult other)
        {
            return Consideration == other.Consideration;
        }

        public override bool Equals(object obj)
        {
            return obj is ConsiderationResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Consideration.GetHashCode();
        }

        public static bool operator ==(ConsiderationResult left, ConsiderationResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConsiderationResult left, ConsiderationResult right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}