// <copyright file="CharacterAnimator.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator animator;
        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        public void PlayDeathAnimation()
        {
            if (animator == null) return;

            animator.SetTrigger(AnimatorParams.Death);
        }
    }
}