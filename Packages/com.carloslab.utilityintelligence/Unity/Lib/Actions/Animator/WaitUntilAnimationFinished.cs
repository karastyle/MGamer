// <copyright file="WaitUntilAnimationFinished.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Animator")]
    public class WaitUntilAnimationFinished : AnimatorActionTask
    {
        public VariableReference<string> AnimationName;
        public float FinishedNormalizedTime = 0.75f;

        public bool IsAnimationFinished
        {
            get
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(AnimationName) && stateInfo.normalizedTime >= FinishedNormalizedTime)
                    return true;

                return false;
            }
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (IsAnimationFinished)
                return UpdateStatus.Success;

            return UpdateStatus.Running;
        }
    }
}