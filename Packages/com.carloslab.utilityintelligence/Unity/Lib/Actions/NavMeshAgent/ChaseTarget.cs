// <copyright file="ChaseTarget.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("NavMeshAgent")]
    public class ChaseTarget : NavMeshActionTask
    {
        public VariableReference<float> Speed = 5;

        protected override void OnStart()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} ChaseTarget OnStart Frame: {Time.frameCount}");

            navMeshAgent.speed = Speed;

            MoveToTarget();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} ChaseTarget OnUpdate Frame: {Time.frameCount}");

            if (HasArrived())
                return UpdateStatus.Success;

            MoveToTarget();

            return UpdateStatus.Running;
        }

        protected override void OnEnd()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} ChaseTarget OnEnd Frame: {Time.frameCount}");

            StopMove();
        }
    }
}