using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class DisToSlot : InputFromSource<float>
    {
        protected override float OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (context.Target.EntityFacade is TargetSlot slot)
                {
                    return Vector3.Distance(enemy.transform.position, slot.transform.position);
                }
            }
            return 0;
        }
    }
}