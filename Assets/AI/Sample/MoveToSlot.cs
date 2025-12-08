

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class MoveToSlot : ActionTask
    {
        private Enemy enemy;
        
        protected override void OnAwake()
        {
            enemy = GetComponent<Enemy>();
        }
        
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var slot = enemy.GetCurrentSlot();
            if (slot != null)
            {
                var slotByMe = this.enemy.IsSlotUsedByMe();
                if (slotByMe)
                {
                    var dis = Vector3.Distance(slot.transform.position, this.Agent.EntityFacade.Position);
                    if (dis < 0.1f)
                    {
                        this.enemy.StopMoving();
                        return UpdateStatus.Success;
                    }
                }
                else
                {
                    this.enemy.StopMoving();
                    return UpdateStatus.Failure;
                }
               
            }
            var bSuc = this.enemy.MoveToSlot(this.TargetEntity.GetComponent<TargetSlot>());
            if (bSuc)
            {
                return UpdateStatus.Running;
            }
            else
            {
                this.enemy.StopMoving();
                return UpdateStatus.Failure;
            }
        }

    }
}