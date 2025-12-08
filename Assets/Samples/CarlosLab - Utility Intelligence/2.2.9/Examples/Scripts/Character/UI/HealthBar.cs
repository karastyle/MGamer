// <copyright file="HealthBar.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.UI;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class HealthBar : CharacterView
    {
        private CharacterHealth characterHealth;
        private Slider healthSlider;
        private TargetFollower targetFollower;

        private void Awake()
        {
            healthSlider = GetComponent<Slider>();
            targetFollower = GetComponent<TargetFollower>();
        }

        public void Init(Transform target)
        {
            targetFollower.FollowTarget(target);

            RotateTowardsCamera();
        }

        public void UpdateHealth(int health)
        {
            healthSlider.value = health;
        }
    }
}