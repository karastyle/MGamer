// <copyright file="RaycastToTargetInput.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    public class RaycastToTargetInput : Input<bool>
    {
        public LayerMask LayerMask;

        public float StartY;
        public float TargetY;
        public VariableReference<float> MaxDistance;

        public bool DebugRaycast;

        protected override bool OnGetRawInput(in InputContext context)
        {
            var startPos = AgentFacade.Position;
            startPos.Y = StartY;


            var targetPos = context.TargetFacade.Position;
            targetPos.Y = TargetY;

            var direction = targetPos - startPos;
            direction.Normalize();


            bool hit = Physics.Raycast(startPos, direction, out RaycastHit raycastHit, MaxDistance, LayerMask);

            Vector3 endPos;
            bool hitTarget = false;

            if (hit)
            {
                endPos = raycastHit.point;

                var targetFacade = raycastHit.transform.GetComponentInParent<IEntityFacade>();
                if (targetFacade == context.TargetFacade)
                    hitTarget = true;
            }
            else
            {
                endPos = startPos + direction * MaxDistance;
            }

            if (DebugRaycast)
                Debug.DrawLine(startPos, endPos, Color.red, 1);

            return hitTarget;
        }
    }
}