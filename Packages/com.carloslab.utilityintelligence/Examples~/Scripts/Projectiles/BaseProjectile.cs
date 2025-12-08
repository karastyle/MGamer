// <copyright file="BaseProjectile.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class BaseProjectile : MonoBehaviour
    {
        protected Rigidbody rigidBody;

        protected float speed;

        protected bool isAttacking;

        private int attackDamage;
        private Team team;

        private ProjectileSoundPlayer soundPlayer;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            soundPlayer = GetComponentInChildren<ProjectileSoundPlayer>();
        }

        public void StartAttack(Vector3 targetPos, int attackDamage, float speed, Team team)
        {
            this.attackDamage = attackDamage;
            this.speed = speed;
            this.team = team;
            isAttacking = true;
            rigidBody.useGravity = false;
            OnStartAttack(targetPos);
        }

        protected virtual void OnStartAttack(Vector3 targetPos)
        {

        }

        private void StopAttack()
        {
            isAttacking = false;
            rigidBody.useGravity = true;
            OnStopAttack();
        }

        protected virtual void OnStopAttack()
        {

        }

        private void OnCollisionEnter(Collision other)
        {
            if (isAttacking)
            {
                var character = other.gameObject.GetComponentInParent<Character>();
                if (character != null)
                {
                    if (character.Team != this.team)
                    {
                        var attackReceiver = character.GetComponent<CharacterAttackTarget>();
                        if (attackReceiver != null)
                            attackReceiver.OnAttacked(attackDamage);
                    }
                }
                else
                {
                    soundPlayer.PlayHitSound();
                }

                StopAttack();
            }
            Destroy(gameObject, 10);
        }
    }
}