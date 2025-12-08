// <copyright file="AgentFilter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public class AgentFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target is UtilityAgent;
        }
    }
}