// <copyright file="RangedAttacker.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public enum RangedAttackType
    {
        Straight,
        Curved
    }

    public class RangedAttacker : CharacterAttacker
    {
        [SerializeField]
        private StraightProjectile straightProjectilePrefab;

        [SerializeField]
        private Transform straightProjectileSpawnPoint;

        [SerializeField]
        private CurvedProjectile curvedProjectilePrefab;

        [SerializeField]
        private Transform curvedProjectileSpawnPoint;

        private RangedAttackType attackType;
        private float projectileSpeed;
        private float maxCurvedHeight;

        public void StartAttackWithStraightProjectile(GameObject target, int consumeEnergy, int attackDamage, float projectileSpeed)
        {
            this.attackType = RangedAttackType.Straight;
            this.projectileSpeed = projectileSpeed;

            StartAttackWithSound(target, consumeEnergy, attackDamage);
        }

        public void StartAttackWithCurvedProjectile(GameObject target, int consumeEnergy, int attackDamage, float projectileSpeed, float maxCurvedHeight)
        {
            this.attackType = RangedAttackType.Curved;
            this.projectileSpeed = projectileSpeed;
            this.maxCurvedHeight = maxCurvedHeight;

            StartAttackWithSound(target, consumeEnergy, attackDamage);
        }

        private void StartAttackWithSound(GameObject target, int consumeEnergy, int attackDamage)
        {
            soundPlayer.PlayRangedAttackSound();

            StartAttack(target, consumeEnergy, attackDamage);
        }

        protected override void OnAttack(GameObject target, int attackDamage)
        {
            var targetPos = target.transform.position;

            switch (attackType)
            {
                case RangedAttackType.Straight:
                    ShootStraightProjectile(targetPos, attackDamage, projectileSpeed);
                    break;
                case RangedAttackType.Curved:
                    ShootCurvedProjectile(targetPos, attackDamage, projectileSpeed, maxCurvedHeight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShootStraightProjectile(Vector3 targetPos, int attackDamage, float projectileSpeed)
        {
            var projectile = Instantiate(straightProjectilePrefab, straightProjectileSpawnPoint.position, Quaternion.identity);
            IgnoreCollisionFromSameTeam(projectile);
            projectile.StartAttack(targetPos, attackDamage, projectileSpeed, character.Team);
        }

        private void ShootCurvedProjectile(Vector3 targetPos, int attackDamage, float projectileSpeed, float maxCurvedHeight)
        {
            var projectile = Instantiate(curvedProjectilePrefab, curvedProjectileSpawnPoint.position, Quaternion.identity);
            IgnoreCollisionFromSameTeam(projectile);
            projectile.StartAttack(targetPos, attackDamage, projectileSpeed, character.Team, maxCurvedHeight);
        }

        private void IgnoreCollisionFromSameTeam(BaseProjectile projectile)
        {
            var projectileCollider = projectile.GetComponentInChildren<Collider>();

            var agents = character.Entity.World.EnabledAgents;
            foreach (var agent in agents)
            {
                if (agent.EntityFacade is Character currentCharacter)
                {
                    if (currentCharacter.Team == character.Team)
                    {
                        var agentCollider = currentCharacter.GetComponentInChildren<Collider>();
                        Physics.IgnoreCollision(projectileCollider, agentCollider);
                    }
                }
            }
        }
    }
}
