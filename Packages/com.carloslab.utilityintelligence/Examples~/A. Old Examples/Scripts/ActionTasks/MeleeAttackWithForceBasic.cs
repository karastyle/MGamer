// <copyright file="MeleeAttackWithForceBasic.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using CarlosLab.UtilityIntelligence.Lib;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class MeleeAttackWithForceBasic : NavMeshActionTask
    {
        #region Fields and Properties

        public VariableReference<float> AttackForce = 10;
        public VariableReference<int> ConsumeEnergy;

        public float AttackSpeed = 20;
        public int AttackDamage = 10;

        private bool isAttacking;
        private float originSpeed;

        private Character character;
        private CharacterEnergy characterEnergy;

        private CharacterAttackTarget attackTarget;

        #endregion

        #region Lifecycle Functions

        protected override void OnAwake()
        {
            base.OnAwake();
            characterEnergy = GetComponent<CharacterEnergy>();
            character = GetComponent<Character>();
        }

        protected override void OnStart()
        {
            originSpeed = navMeshAgent.speed;
            attackTarget = TargetGameObject.GetComponent<CharacterAttackTarget>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (!isAttacking)
            {
                // if (character.State == CharacterState.Attacked)
                // {
                //     //Debug.Log($"Agent: {Agent.Name} BasicAttack OnUpdate Failed State: {character.State} Frame: {Time.frameCount}");
                //     return UpdateStatus.Failure;
                // }
                // StartAttack(TargetGameObject);
                // isAttacking = true;

                if (character.State == CharacterState.Normal)
                {
                    StartAttack(TargetGameObject);
                    isAttacking = true;
                }
            }
            else
            {
                Vector3 vector = TargetTransform.position - Transform.position;
                vector.y = 0;

                float distanceSqr = vector.sqrMagnitude;
                Vector3 direction = vector.normalized;

                if (distanceSqr <= 2.0f)
                {
                    attackTarget.OnAttacked(AttackDamage, AttackForce, direction);

                    return UpdateStatus.Success;
                }
                else if (character.State == CharacterState.Attacked || HasArrived())
                {
                    return UpdateStatus.Failure;
                }
            }

            return UpdateStatus.Running;
        }

        private void StartAttack(GameObject target)
        {
            characterEnergy.Energy -= ConsumeEnergy;

            navMeshAgent.speed = AttackSpeed;
            Vector3 targetPosition = target.transform.position;
            targetPosition.y = 0;
            StartMove(targetPosition);
        }

        protected override void OnEnd()
        {
            isAttacking = false;
            navMeshAgent.speed = originSpeed;
            StopMove();
        }

        #endregion
    }
}