

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class IsSlotTypeMatch : InputFromSource<bool>
    {
        protected override bool OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (context.Target.EntityFacade is TargetSlot slot)
                {
                    var isNear = enemy.enemyType == EnemyType.Near;
                    if (isNear)
                    {
                        return slot.slotType == TargetSlotType.Attack || slot.slotType == TargetSlotType.Keep;
                    }
                    else
                    {
                        return slot.slotType == TargetSlotType.Shoot;
                    }
                }
            }

            return false;
        }
    }
}