// <copyright file="ChargeHealth.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class ChargeHealth : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (TargetFacade is ChargeStation { Type: ChargeStationType.HealthStation } healthStation)
            {
                int chargeHealth = healthStation.ChargePerSec;
                CharacterHealth health = GetComponent<CharacterHealth>();
                health.Health += chargeHealth;

                return UpdateStatus.Success;
            }

            return UpdateStatus.Failure;
        }
    }
}