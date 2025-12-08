// <copyright file="CharacterDeath.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterDeath : MonoBehaviour
    {
        [SerializeField]
        private float destroyDelayTime;

        private Character character;
        private CharacterHealth health;
        private CharacterAnimator animator;
        private CharacterSoundPlayer soundPlayer;

        private bool isDied;
        private void Awake()
        {
            character = GetComponent<Character>();
            health = GetComponent<CharacterHealth>();
            animator = GetComponent<CharacterAnimator>();
            soundPlayer = GetComponentInChildren<CharacterSoundPlayer>();
        }

        private void OnEnable()
        {
            health.OutOfHealth += OnOutOfHealth;
        }

        private void OnDisable()
        {
            health.OutOfHealth -= OnOutOfHealth;
        }

        private void OnOutOfHealth()
        {
            Die();
        }

        private void Die()
        {
            if (isDied)
                return;

            isDied = true;

            PlayDeathSound();
            PlayDeathAnimation();

            DestroySelf();
        }

        private void PlayDeathAnimation()
        {
            if (animator == null) return;

            animator.PlayDeathAnimation();
        }

        private void PlayDeathSound()
        {
            if (soundPlayer == null) return;

            soundPlayer.PlayDeathSound();
        }

        private void DestroySelf()
        {
            character.Unregister();
            character.DestroyAfter(destroyDelayTime);
        }
    }
}