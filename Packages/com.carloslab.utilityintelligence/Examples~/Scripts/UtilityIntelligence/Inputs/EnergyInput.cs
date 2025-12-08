// <copyright file="EnergyInput.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class EnergyInput : InputFromSource<int>
    {
        protected override int OnGetRawInput(in InputContext context)
        {
            UtilityEntity inputSource = GetInputSource(in context);
            if (inputSource.EntityFacade is Character character)
            {
                return character.Energy;
            }

            return 0;
        }
    }
}