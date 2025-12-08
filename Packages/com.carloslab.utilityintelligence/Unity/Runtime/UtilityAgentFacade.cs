// <copyright file="UtilityAgentFacade.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [AddComponentMenu(FrameworkConsts.AddAgentFacadeMenuPath)]
    public class UtilityAgentFacade : EntityFacade<UtilityAgent>
    {
        public void Register(UtilityWorld world)
        {
            Entity?.Register(world);
        }

        public void RegisterImmediate(UtilityWorldController world)
        {
            Entity?.RegisterImmediate(world.World);
        }
    }
}