// <copyright file="UtilityEntityController.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [RequireComponent(typeof(UtilityEntityFacade))]
    [AddComponentMenu(FrameworkConsts.AddEntityControllerMenuPath)]
    public class UtilityEntityController : EntityController<UtilityEntity>
    {
        public UtilityWorld World => entity?.World;

        protected override UtilityEntity CreateEntity()
        {
            return new UtilityEntity();
        }

        public void Register(UtilityWorldController world)
        {
            entity?.Register(world.World);
        }

        public void Register(UtilityWorld world)
        {
            entity?.Register(world);
        }
    }
}