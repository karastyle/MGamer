// <copyright file="DistanceToTargetInput.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    public class DistanceToTargetInput : Input<float>
    {
        protected override float OnGetRawInput(in InputContext context)
        {
            var currentPos = AgentFacade.Position;
            var targetPos = context.TargetFacade.Position;
            currentPos.Y = 0;
            targetPos.Y = 0;

            return Vector3.Distance(currentPos, targetPos);
        }
    }
}