// <copyright file="OtherTeamFilter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class TargetSlotFilter : TargetFilter
    {
        //是否要未使用的插槽
        public bool needNotUsed = true;
        //是否包含当前插槽
        public bool includeCurrentSlot = true;
        
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (target.EntityFacade is TargetSlot slot)
                {
                    if (needNotUsed)
                    {
                        if (includeCurrentSlot)
                        {
                            if( enemy.IsTargetSlotUsedByMe(slot) )
                            {
                                return true;
                            }
                        }
                        return slot.usedEnemeyId == 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}