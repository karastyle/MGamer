using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class HasSlotUsedByMe : InputFromSource<bool>
    {
        protected override bool OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                var used = enemy.IsSlotUsedByMe();
                return used;
            }

            return false;
        }
    }
}