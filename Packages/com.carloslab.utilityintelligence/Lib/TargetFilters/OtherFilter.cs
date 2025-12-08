// <copyright file="OtherFilter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public class OtherFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target != this.Agent;
        }
    }
}