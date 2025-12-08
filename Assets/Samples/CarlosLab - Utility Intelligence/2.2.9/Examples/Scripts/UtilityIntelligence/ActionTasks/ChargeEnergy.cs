// <copyright file="ChargeEnergy.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class ChargeEnergy : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (TargetFacade is ChargeStation { Type: ChargeStationType.EnergyStation } energyStation)
            {
                int chargeEnergy = energyStation.ChargePerSec;
                CharacterEnergy energy = GetComponent<CharacterEnergy>();
                energy.Energy += chargeEnergy;

                return UpdateStatus.Success;
            }

            return UpdateStatus.Failure;
        }
    }
}