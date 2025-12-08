// <copyright file="InfluenceCurve.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public struct InfluenceCurve : IEquatable<InfluenceCurve>
    {
        public InfluenceCurveType Type;

        public float Slope;

        public float Exponent;

        public float XShift;

        public float YShift;

        public InfluenceCurve(InfluenceCurveType type, float slope, float exponent, float xShift, float yShift)
        {
            Type = type;
            Slope = slope;
            Exponent = exponent;
            XShift = xShift;
            YShift = yShift;
        }

        public override string ToString()
        {
            return $"{Type} {Slope} {Exponent} {XShift} {YShift}";
        }

        public static readonly InfluenceCurve BasicLinear = new(InfluenceCurveType.Linear, 1, 0, 0, 0);
        public static readonly InfluenceCurve InverseLinear = new(InfluenceCurveType.Linear, -1, 0, 0, 1);
        public static readonly InfluenceCurve Constant = new(InfluenceCurveType.Linear, 0, 0, 0, 0.5f);
        public static readonly InfluenceCurve BasicLogistic = new(InfluenceCurveType.Logistic, 1, 1, 0, 0);
        public static readonly InfluenceCurve InverseLogistic = new(InfluenceCurveType.Logistic, -1, 1, 0, 1);
        public static readonly InfluenceCurve BasicLogit = new(InfluenceCurveType.Logit, 1, 1, 0, 0);
        public static readonly InfluenceCurve InverseLogit = new(InfluenceCurveType.Logit, -1, 1, 0, 0);
        public static readonly InfluenceCurve BasicQuadricLowerLeft = new(InfluenceCurveType.Polynomial, 1, 4, 1, 0);
        public static readonly InfluenceCurve BasicQuadricLowerRight = new(InfluenceCurveType.Polynomial, 1, 4, 0, 0);
        public static readonly InfluenceCurve BasicQuadricUpperLeft = new(InfluenceCurveType.Polynomial, -1, 4, 1, 1);
        public static readonly InfluenceCurve BasicQuadricUpperRight = new(InfluenceCurveType.Polynomial, -1, 4, 0, 1);
        public static readonly InfluenceCurve BasicSine = new(InfluenceCurveType.Sine, 1, 1, 0, 0);
        public static readonly InfluenceCurve InverseSine = new(InfluenceCurveType.Sine, -1, 1, 0, 0);
        public static readonly InfluenceCurve BasicBellCurve = new(InfluenceCurveType.Normal, 1, 1, 0, 0);
        public static readonly InfluenceCurve InverseBellCurve = new(InfluenceCurveType.Normal, -1, 1, 0, 1);

        public bool Equals(InfluenceCurve other)
        {
            return Type == other.Type
                   && Math.Abs(Slope - other.Slope) < MathUtils.Epsilon
                   && Math.Abs(Exponent - other.Exponent) < MathUtils.Epsilon
                   && Math.Abs(XShift - other.XShift) < MathUtils.Epsilon
                   && Math.Abs(YShift - other.YShift) < MathUtils.Epsilon;
        }

        public override bool Equals(object obj)
        {
            return obj is InfluenceCurve other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, Slope, Exponent, XShift, YShift);
        }

        public static bool operator ==(InfluenceCurve left, InfluenceCurve right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InfluenceCurve left, InfluenceCurve right)
        {
            return !left.Equals(right);
        }
    }
}