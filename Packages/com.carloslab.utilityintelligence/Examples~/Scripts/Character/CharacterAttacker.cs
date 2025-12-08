// <copyright file="CharacterAttacker.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public abstract class CharacterAttacker : MonoBehaviour
    {
        private CharacterAnimationEvents animationEvents;

        protected Character character;
        protected CharacterSoundPlayer soundPlayer;

        private CharacterEnergy energy;

        private GameObject target;
        private int attackDamage;
        private bool isAttacking;

        protected virtual void Awake()
        {
            animationEvents = GetComponentInChildren<CharacterAnimationEvents>();
            character = GetComponent<Character>();
            energy = GetComponent<CharacterEnergy>();
            soundPlayer = GetComponentInChildren<CharacterSoundPlayer>();
        }

        private void OnEnable()
        {
            animationEvents.Attack += AttackEventHandler;
        }

        private void OnDisable()
        {
            animationEvents.Attack -= AttackEventHandler;
        }

        protected void StartAttack(GameObject target, int consumeEnergy, int attackDamage)
        {
            this.target = target;
            this.attackDamage = attackDamage;

            if (character.State != CharacterState.Normal || energy.Energy < consumeEnergy)
                return;

            energy.Energy -= consumeEnergy;
            isAttacking = true;
        }

        public void StopAttack()
        {
            isAttacking = false;
            target = null;
        }

        protected virtual void AttackEventHandler()
        {
            if (isAttacking == false || target == null) return;

            OnAttack(target, attackDamage);
        }

        protected virtual void OnAttack(GameObject target, int attackDamage)
        {
        }
    }
}