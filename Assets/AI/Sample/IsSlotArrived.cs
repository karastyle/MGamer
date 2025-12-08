using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class IsSlotArrived : InputFromSource<bool>
    {
        protected override bool OnGetRawInput(in InputContext context)
        {
            if (context.Target.EntityFacade is TargetSlot slot)
            {
                var enemy = this.Agent.EntityFacade as Enemy;
                var usedByMe = enemy.IsSlotUsedByMe();
                if (usedByMe)
                {
                    var isArr = enemy.IsArrived();
                    return isArr;
                }
            }
            return false;
        }
    }
}