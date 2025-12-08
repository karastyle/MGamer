// <copyright file="FaceTargetForever.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("3D")]
    public class FaceTargetForever : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var direction = TargetTransform.position - Transform.position;
            direction.Normalize();
            direction.y = 0;

            if (direction != Vector3.zero)
                Transform.forward = direction;

            return UpdateStatus.Running;
        }
    }
}