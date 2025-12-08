// <copyright file="InputNormalizationResult.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct InputNormalizationResult : IEquatable<InputNormalizationResult>
    {
        public static readonly InputNormalizationResult Null = new();

        public InputNormalizationResult(InputNormalization inputNormalization) : this()
        {
            InputNormalization = inputNormalization;
        }

        public readonly InputNormalization InputNormalization;

        public object RawInput;
        public float NormalizedInput;

        #region IEquatable

        public bool Equals(InputNormalizationResult other)
        {
            return InputNormalization == other.InputNormalization;
        }

        public override bool Equals(object obj)
        {
            return obj is InputNormalizationResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return InputNormalization.GetHashCode();
        }

        public static bool operator ==(InputNormalizationResult left, InputNormalizationResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputNormalizationResult left, InputNormalizationResult right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}