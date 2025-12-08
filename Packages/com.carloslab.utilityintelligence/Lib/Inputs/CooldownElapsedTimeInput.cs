// <copyright file="CooldownElapsedTimeInput.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class CooldownElapsedTimeInput : Input<float>
    {
        public VariableReference<float> CooldownStartTime;

        protected override float OnGetRawInput(in InputContext context)
        {
            float elapsedTime = WorldTime.Instance.Time - CooldownStartTime;
            return elapsedTime;
        }
    }
}