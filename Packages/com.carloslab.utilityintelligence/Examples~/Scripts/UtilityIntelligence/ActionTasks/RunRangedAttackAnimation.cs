// <copyright file="RunRangedAttackAnimation.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using CarlosLab.UtilityIntelligence.Lib;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class RunRangedAttackAnimation : WaitUntilAnimationFinished
    {
        public VariableReference<string> AttackParamName;

        protected override void OnStart()
        {
            animator.SetBool(AttackParamName, true);
        }

        protected override void OnEnd()
        {
            animator.SetBool(AttackParamName, false);
        }
    }
}