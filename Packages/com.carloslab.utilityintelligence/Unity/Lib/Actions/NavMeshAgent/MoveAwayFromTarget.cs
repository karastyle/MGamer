// <copyright file="MoveAwayFromTarget.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("NavMeshAgent")]
    public class MoveAwayFromTarget : NavMeshActionTask
    {
        public float Speed = 5;

        public float DistanceToNextPoint = 5;
        public bool DebugNextPoint;

        private int directionPriorityIndex;
        private Vector3 nextPoint;
        private MoveAwayState currentState;

        protected override void OnStart()
        {
            navMeshAgent.speed = Speed;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            switch (currentState)
            {
                case MoveAwayState.FindNextPoint:
                    if (TryFindNextPoint(out Vector3 nextPoint))
                    {
                        currentState = MoveAwayState.MoveToNextPoint;
                        StartMove(nextPoint);
                    }
                    break;
                case MoveAwayState.MoveToNextPoint:
                    if (HasArrived())
                        return UpdateStatus.Success;
                    break;
            }

            return UpdateStatus.Running;
        }

        private bool TryFindNextPoint(out Vector3 nextPoint)
        {
            Vector3 direction = Vector3.zero;
            DirectionPriority directionType = (DirectionPriority)directionPriorityIndex;

            Vector3 backward = TargetTransform.forward;
            Vector3 left = TargetTransform.right;
            Vector3 right = -TargetTransform.right;
            Vector3 forward = -TargetTransform.forward;

            switch (directionType)
            {
                case DirectionPriority.Backward:
                    direction = backward;
                    break;
                case DirectionPriority.BackwardLeft:
                    direction = backward + left;
                    break;
                case DirectionPriority.BackwardRight:
                    direction = backward + right;
                    break;
                case DirectionPriority.Left:
                    direction = left;
                    break;
                case DirectionPriority.Right:
                    direction = right;
                    break;
                case DirectionPriority.ForwardLeft:
                    direction = forward + left;
                    break;
                case DirectionPriority.ForwardRight:
                    direction = forward + right;
                    break;
                case DirectionPriority.Forward:
                    direction = forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            direction.Normalize();

            var targetPoint = Transform.position + direction * DistanceToNextPoint;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPoint, out hit, 1, NavMesh.AllAreas))
            {
                nextPoint = hit.position;

                if (DebugNextPoint)
                    DebugUtils.DrawPoint(nextPoint, Color.red, 1, 1);
                return true;
            }
            else
            {
                nextPoint = Vector3.zero;
                int maxDirectionPriorityIndex = (int)DirectionPriority.Forward;
                if (directionPriorityIndex < maxDirectionPriorityIndex)
                    directionPriorityIndex++;
                else
                    directionPriorityIndex = 0;

                return false;
            }
        }

        protected override void OnEnd()
        {
            StopMove();
            currentState = MoveAwayState.FindNextPoint;
            directionPriorityIndex = 0;
        }

        private enum DirectionPriority
        {
            Backward,
            BackwardLeft,
            BackwardRight,
            Left,
            Right,
            ForwardLeft,
            ForwardRight,
            Forward,
        }

        private enum MoveAwayState
        {
            FindNextPoint,
            MoveToNextPoint
        }
    }
}
