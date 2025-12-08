// <copyright file="HealthInput.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class HealthInput : InputFromSource<int>
    {
        protected override int OnGetRawInput(in InputContext context)
        {
            UtilityEntity inputSource = GetInputSource(in context);
            if (inputSource.EntityFacade is Character character)
            {
                return character.Health;
            }

            return 0;
        }
    }
}