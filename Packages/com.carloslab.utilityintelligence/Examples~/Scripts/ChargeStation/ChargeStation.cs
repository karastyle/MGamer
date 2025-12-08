// <copyright file="ChargeStation.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class ChargeStation : UtilityEntityFacade
    {
        [SerializeField]
        private ChargeStationType type;

        [SerializeField]
        private float chargeRadius;

        [SerializeField]
        private int chargePerSec;

        public ChargeStationType Type => type;
        public float ChargeRadius => chargeRadius;
        public int ChargePerSec => chargePerSec;
    }
}