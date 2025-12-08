

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class MoveToEmptyArea : ActionTask
    {
        private Enemy enemy;
        
        protected override void OnAwake()
        {
            enemy = GetComponent<Enemy>();
        }
        
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            this.enemy.StartGradientDescent();
            return UpdateStatus.Running;
        }

    }
}