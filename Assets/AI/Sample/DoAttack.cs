// <copyright file="ChargeHealth.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class DoAttack : ActionTask
    {
        private Enemy enemy;
        
        protected override void OnAwake()
        {
            enemy = GetComponent<Enemy>();
            this.enemy.DoAttack();
        }
        
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Running;
        }

    }
}