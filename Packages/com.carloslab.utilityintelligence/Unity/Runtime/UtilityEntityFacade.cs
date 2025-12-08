// <copyright file="UtilityEntityFacade.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [AddComponentMenu(FrameworkConsts.AddEntityFacadeMenuPath)]
    public class UtilityEntityFacade : EntityFacade<UtilityEntity>
    {
        public void Register(UtilityWorld world)
        {
            Entity?.Register(world);
        }
    }
}