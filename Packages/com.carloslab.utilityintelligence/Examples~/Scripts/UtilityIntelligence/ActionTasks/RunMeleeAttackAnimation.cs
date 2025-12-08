// <copyright file="RunMeleeAttackAnimation.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using CarlosLab.UtilityIntelligence.Lib;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [ClassFormerlySerializedAs(oldClassName: "RunAttackAnimation")]
    [Category("Examples")]
    public class RunMeleeAttackAnimation : WaitUntilAnimationFinished
    {
        public VariableReference<int> AttackNumber;
        public VariableReference<string> AttackParamName;

        protected override void OnStart()
        {
            animator.SetInteger(AttackParamName, AttackNumber);
        }

        protected override void OnEnd()
        {
            animator.SetInteger(AttackParamName, 0);
        }
    }
}