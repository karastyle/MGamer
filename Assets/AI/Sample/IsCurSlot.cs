using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class IsCurSlot : InputFromSource<bool>
    {
        protected override bool OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (context.Target.EntityFacade is TargetSlot slot)
                {
                    var used = enemy.IsSlotUsedByMe();
                    return used;
                }
            }

            return false;
        }
    }
}