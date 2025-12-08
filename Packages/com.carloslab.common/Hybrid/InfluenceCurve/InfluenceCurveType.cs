// <copyright file="InfluenceCurveType.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public enum InfluenceCurveType : byte
    {
        Linear,
        Polynomial,
        Logistic,
        Logit,
        Normal,
        Sine
    }
}