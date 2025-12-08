// <copyright file="Patrol.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("NavMeshAgent")]
    public class Patrol : NavMeshActionTask
    {
        public VariableReference<float> Speed = 5;

        public VariableReference<List<Transform>> Waypoints;

        private int waypointIndex;

        protected override void OnStart()
        {
            navMeshAgent.speed = Speed;

            MoveToNextWaypoint();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (HasArrived())
                MoveToNextWaypoint();

            return UpdateStatus.Running;
        }

        protected override void OnEnd()
        {
            waypointIndex = 0;
        }

        private void MoveToNextWaypoint()
        {
            var nextWaypoint = GetNextWaypoint();
            StartMove(nextWaypoint);
        }

        private Vector3 GetNextWaypoint()
        {
            var waypoint = Waypoints.Value[waypointIndex];

            if (waypointIndex < Waypoints.Value.Count - 1)
            {
                waypointIndex++;
            }
            else
            {
                waypointIndex = 0;
            }

            return waypoint.position;
        }
    }
}