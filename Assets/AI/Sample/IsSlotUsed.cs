using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class IsSlotUsed : InputFromSource<bool>
    {
        //是否包含当前插槽
        public bool includeCurrentSlot = true;
        
        protected override bool OnGetRawInput(in InputContext context)
        {
            if (context.Target.EntityFacade is TargetSlot slot)
            {
                return slot.usedEnemeyId > 0;
            }

            return false;
        }
    }
}