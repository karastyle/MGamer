// <copyright file="ChargeHealth.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class IdleToSlot : ActionTask
    {
        private Enemy enemy;
        
        protected override void OnAwake()
        {
            enemy = GetComponent<Enemy>();
            enemy.StopMoving();
        }
        
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Running;
        }

    }
}