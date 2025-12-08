// <copyright file="StartCooldown.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    public class StartCooldown : ActionTask
    {
        public VariableReference<float> CooldownStartTime;

        protected override void OnStart()
        {
            CooldownStartTime.Value = Time.time;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Success;
        }
    }
}