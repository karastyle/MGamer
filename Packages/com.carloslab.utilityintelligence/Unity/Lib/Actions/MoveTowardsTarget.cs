// <copyright file="MoveTowardsTarget.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("3D")]
    public class MoveTowardsTarget : ActionTask
    {
        public VariableReference<float> Speed = 5;
        public float StoppingDistance = 0.5f;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            TaskConsole.Instance.Log(
                $"Entity: {Agent.Name} Target: {Context.Target.Name} MoveToTarget");
            Transform transform = GetComponent<Transform>();
            Vector3 myPosition = transform.position;
            Vector3 targetPosition = TargetTransform.position;

            if (Vector3.Distance(myPosition, targetPosition) <= StoppingDistance)
                return UpdateStatus.Success;

            transform.position = Vector3.MoveTowards(myPosition, targetPosition, Speed * Time.deltaTime);

            return UpdateStatus.Running;
        }
    }
}