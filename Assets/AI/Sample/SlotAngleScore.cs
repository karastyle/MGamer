using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class SlotAngleScore : InputFromSource<float>
    {
        protected override float OnGetRawInput(in InputContext context)
        {
            if (this.Agent.EntityFacade is Enemy enemy)
            {
                if (context.Target.EntityFacade is TargetSlot slot)
                {
                    var toSlot = (slot.transform.position - enemy.transform.position).normalized;
                    var player = AIGameManager.Player;
                    if (player != null)
                    {
                        var forward = player.transform.forward;
                        var angle = Vector3.Angle(forward, toSlot);
                        var score = 1.0f - (angle / 180.0f);
                        return score;
                    }
                }
            }

            return 0;
        }
    }
}