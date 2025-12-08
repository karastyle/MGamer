// <copyright file="MeleeAttacker.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public enum MeleeAttackType
    {
        AttackWithoutForce,
        AttackWithForce
    }
    public class MeleeAttacker : CharacterAttacker
    {
        private MeleeAttackType attackType;

        private float attackRange;
        private float attackForce;

        public void StartAttackWithoutForce(GameObject target
            , int consumeEnergy
            , int attackDamage
            , float attackRange)
        {
            this.attackType = MeleeAttackType.AttackWithoutForce;
            this.attackRange = attackRange;

            StartAttackWithSound(target, consumeEnergy, attackDamage);
        }

        public void StartAttackWithForce(GameObject target
            , int consumeEnergy
            , int attackDamage
            , float attackRange
            , float attackForce)
        {
            this.attackType = MeleeAttackType.AttackWithForce;
            this.attackRange = attackRange;
            this.attackForce = attackForce;

            StartAttackWithSound(target, consumeEnergy, attackDamage);
        }

        private void StartAttackWithSound(GameObject target, int consumeEnergy, int attackDamage)
        {
            soundPlayer.PlayMeleeAttackSound();

            StartAttack(target, consumeEnergy, attackDamage);
        }

        protected override void OnAttack(GameObject target, int attackDamage)
        {
            var currentPos = transform.position;
            var targetPos = target.transform.position;
            currentPos.y = 0;
            targetPos.y = 0;

            float distance = Vector3.Distance(currentPos, targetPos);

            if (distance <= attackRange)
            {
                CharacterAttackTarget attackTarget = target.GetComponent<CharacterAttackTarget>();

                switch (attackType)
                {
                    case MeleeAttackType.AttackWithoutForce:
                        attackTarget.OnAttacked(attackDamage);
                        break;
                    case MeleeAttackType.AttackWithForce:
                        var direction = target.transform.position - transform.position;
                        direction.Normalize();
                        direction.y = 0;

                        attackTarget.OnAttacked(attackDamage, attackForce, direction);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}