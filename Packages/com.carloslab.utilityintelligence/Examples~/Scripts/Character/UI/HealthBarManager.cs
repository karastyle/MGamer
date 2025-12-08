// <copyright file="HealthBarManager.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class HealthBarManager : MonoBehaviour
    {
        [SerializeField]
        private HealthBar healthBarPrefab;

        private CharacterHealth characterHealth;

        private HealthBar healthBar;
        private Transform worldCanvas;

        private void Awake()
        {
            worldCanvas = GameObject.Find("WorldCanvas").transform;
            characterHealth = GetComponentInParent<CharacterHealth>();
        }

        private void OnEnable()
        {
            characterHealth.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            characterHealth.HealthChanged -= OnHealthChanged;
        }

        private void Start()
        {
            healthBar = Instantiate(healthBarPrefab, worldCanvas, false);
            healthBar.Init(transform);
        }

        private void OnHealthChanged(int health)
        {
            healthBar.UpdateHealth(health);
        }

        private void OnDestroy()
        {
            if (healthBar != null)
                Destroy(healthBar.gameObject);
        }
    }
}