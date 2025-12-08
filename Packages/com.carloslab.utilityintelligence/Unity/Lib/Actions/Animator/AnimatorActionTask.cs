// <copyright file="AnimatorActionTask.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    public abstract class AnimatorActionTask : ActionTask
    {
        public VariableReference<Animator> Animator;

        protected Animator animator => Animator.Value;

        protected override void OnAwake()
        {
            if (Animator.Value == null)
                Animator.Value = GetComponentInChildren<Animator>();
        }
    }
}