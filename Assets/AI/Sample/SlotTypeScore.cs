using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class SlotTypeScore : InputFromSource<float>
    {
        protected override float OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (context.Target.EntityFacade is TargetSlot slot)
                {
                    return enemy.GetScoreBySlotType(slot.slotType);
                }
            }

            return 0;
        }
    }
}