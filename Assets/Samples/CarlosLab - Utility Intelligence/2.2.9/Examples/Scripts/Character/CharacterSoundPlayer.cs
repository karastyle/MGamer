// <copyright file="CharacterSoundPlayer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterSoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] meleeAttackSounds;

        [SerializeField]
        private AudioClip[] rangedAttackSounds;

        [SerializeField]
        private AudioClip[] hitSounds;

        [SerializeField]
        private AudioClip[] deathSounds;

        [SerializeField]
        private AudioClip[] footstepSounds;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayMeleeAttackSound()
        {
            AudioClip clip = meleeAttackSounds[Random.Range(0, meleeAttackSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        public void PlayRangedAttackSound()
        {
            AudioClip clip = rangedAttackSounds[Random.Range(0, rangedAttackSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        public void PlayHitSound()
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        public void PlayDeathSound()
        {
            AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        public void PlayFootstepSound()
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}