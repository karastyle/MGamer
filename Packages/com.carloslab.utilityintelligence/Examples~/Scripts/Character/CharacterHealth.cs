// <copyright file="CharacterHealth.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterHealth : MonoBehaviour
    {
        private Character character;

        private int health = 100;

        public event Action<int> HealthChanged;

        public event Action OutOfHealth;

        private void Awake()
        {
            character = GetComponent<Character>();
        }

        public int Health
        {
            get => health;
            set
            {
                int newHealth = value;

                if (newHealth < 0)
                    newHealth = 0;

                if (this.health == newHealth)
                    return;

                this.health = newHealth;
                HealthChanged?.Invoke(this.health);

                if (this.health == 0)
                    OutOfHealth?.Invoke();
            }
        }
    }
}