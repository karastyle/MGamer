// <copyright file="InfluenceCurveConverter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

#if UNITY_EDITOR

using System;
using UnityEditor.UIElements;

namespace CarlosLab.Common.UI
{
    public class InfluenceCurveConverter : UxmlAttributeConverter<InfluenceCurve>
    {
        public override InfluenceCurve FromString(string value)
        {
            string[] split = value.Split('|');

            return new InfluenceCurve
            {
                Type = (InfluenceCurveType)Enum.Parse(typeof(InfluenceCurve), split[0]),
                Slope = float.Parse(split[1])
            };
        }

        public override string ToString(InfluenceCurve value)
        {
            return $"{value.Type}|{value.Slope}|{value.Exponent}|{value.XShift}|{value.YShift}";
        }
    }
}

#endif


