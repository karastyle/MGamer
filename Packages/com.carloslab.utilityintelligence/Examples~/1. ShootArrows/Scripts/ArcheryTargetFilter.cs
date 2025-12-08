// <copyright file="ArcheryTargetFilter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class ArcheryTargetFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target.EntityFacade is ArcheryTarget;
        }
    }
}
