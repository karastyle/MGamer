// <copyright file="ChargeStationFilter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class ChargeStationFilter : TargetFilter
    {
        public ChargeStationType Type;

        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target.EntityFacade is ChargeStation station && station.Type == Type;
        }
    }
}