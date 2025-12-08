// <copyright file="StopAttack.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class StopAttack : ActionTask
    {
        private CharacterAttacker attacker;
        protected override void OnAwake()
        {
            attacker = GetComponent<CharacterAttacker>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            attacker.StopAttack();
            return UpdateStatus.Success;
        }
    }
}