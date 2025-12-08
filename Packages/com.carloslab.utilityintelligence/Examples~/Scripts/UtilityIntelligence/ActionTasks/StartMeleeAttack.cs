// <copyright file="StartMeleeAttack.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class StartMeleeAttack : ActionTask
    {
        public MeleeAttackType AttackType;
        public VariableReference<float> AttackRange;
        public int AttackDamage;
        public int ConsumeEnergy;

        [ShowIf("AttackType", MeleeAttackType.AttackWithForce)]
        public int AttackForce;

        [FoldoutGroup("Animation")]
        public VariableReference<int> AttackNumber;

        [FoldoutGroup("Animation")]
        public VariableReference<string> AttackAnimationName;

        protected MeleeAttacker attacker;
        protected override void OnAwake()
        {
            attacker = GetComponent<MeleeAttacker>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            AttackNumber.Value++;
            if (AttackNumber.Value > 4)
                AttackNumber.Value = 1;

            AttackAnimationName.Value = "Attack" + AttackNumber.Value;

            StartAttack(TargetGameObject, ConsumeEnergy, AttackDamage, AttackRange);
            return UpdateStatus.Success;
        }

        private void StartAttack(GameObject target, int consumeEnergy, int attackDamage, float attackRange)
        {
            if (AttackType == MeleeAttackType.AttackWithoutForce)
            {
                attacker.StartAttackWithoutForce(target, consumeEnergy, attackDamage, attackRange);
            }
            else
            {
                attacker.StartAttackWithForce(target, consumeEnergy, attackDamage, attackRange, AttackForce);
            }
        }
    }
}