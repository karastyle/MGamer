// <copyright file="InputResult.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public struct InputResult<TValue> : IEquatable<InputResult<TValue>>
    {
        public static readonly InputResult<TValue> Null = new();

        public InputResult(Input<TValue> input) : this()
        {
            Input = input;
        }

        public readonly Input<TValue> Input;

        public TValue RawInput;

        #region IEquatable

        public bool Equals(InputResult<TValue> other)
        {
            return Input == other.Input;
        }

        public override bool Equals(object obj)
        {
            return obj is InputResult<TValue> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Input.GetHashCode();
        }

        public static bool operator ==(InputResult<TValue> left, InputResult<TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputResult<TValue> left, InputResult<TValue> right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}