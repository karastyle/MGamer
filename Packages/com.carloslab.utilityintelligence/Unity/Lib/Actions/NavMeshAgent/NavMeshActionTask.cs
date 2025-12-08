// <copyright file="NavMeshActionTask.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.AI;

namespace CarlosLab.UtilityIntelligence.Lib
{
    public abstract class NavMeshActionTask : ActionTask
    {
        public VariableReference<NavMeshAgent> NavMeshAgent;

        protected NavMeshAgent navMeshAgent => NavMeshAgent.Value;

        protected float RemainingDistance
        {
            get
            {
                float remainingDistance = float.PositiveInfinity;

                if (navMeshAgent.isActiveAndEnabled)
                {
                    if (!navMeshAgent.pathPending)
                        remainingDistance = navMeshAgent.remainingDistance;
                }

                return remainingDistance;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (NavMeshAgent.Value == null)
                NavMeshAgent.Value = GetComponentInChildren<NavMeshAgent>();
        }

        protected bool HasArrived()
        {
            bool hasArrived = RemainingDistance <= navMeshAgent.stoppingDistance;
            return hasArrived;
        }

        protected void StartMove(Vector3 destination)
        {
            if (!navMeshAgent.isActiveAndEnabled) return;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
        }

        protected void MoveToTarget()
        {
            Vector3 targetPosition = TargetTransform.position;
            // targetPosition.y = 0;
            StartMove(targetPosition);
        }

        protected void StopMove()
        {
            if (!navMeshAgent.isActiveAndEnabled) return;

            navMeshAgent.isStopped = true;
        }

        protected void SetUpdateRotation(bool update)
        {
            navMeshAgent.updateRotation = update;
            navMeshAgent.updateUpAxis = update;
        }
    }
}