// <copyright file="IsCharacterStateNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class IsCharacterStateNormalization : InputNormalization<CharacterState>
    {
        public CharacterState State;
        protected override float OnCalculateNormalizedInput(CharacterState rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = rawInput == State ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }
}