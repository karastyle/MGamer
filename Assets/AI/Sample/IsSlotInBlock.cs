using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class IsSlotInBlock : InputFromSource<bool>
    {
        public float maxDistance = 1f;
        
        protected override bool OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (context.Target.EntityFacade is TargetSlot slot)
                {
                    var walkable = enemy.IsPositionWalkable(slot.transform.position, maxDistance);
                    return ! walkable;
                }
            }

            return false;
        }
    }
}