// <copyright file="CharacterAttackTarget.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterAttackTarget : MonoBehaviour
    {
        [SerializeField]
        private GameObject straightProjectileTargetPoint;
        public GameObject StraightProjectileTargetPoint => straightProjectileTargetPoint;

        [SerializeField]
        private GameObject curvedProjectileTargetPoint;
        public GameObject CurvedProjectileTargetPoint => curvedProjectileTargetPoint;

        private Coroutine backToNormalStateCoroutine;
        private Character character;

        private NavMeshAgent navMeshAgent;
        private Rigidbody rigidBody;
        private CharacterHealth health;
        private CharacterSoundPlayer soundPlayer;

        private void Awake()
        {
            character = GetComponent<Character>();
            health = GetComponent<CharacterHealth>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();
            soundPlayer = GetComponentInChildren<CharacterSoundPlayer>();
        }

        private void PlayHitSound()
        {
            if (soundPlayer == null) return;

            soundPlayer.PlayHitSound();
        }

        public void OnAttacked(int damage)
        {
            PlayHitSound();

            health.Health -= damage;
        }

        public void OnAttacked(int damage, float force, Vector3 direction)
        {
            PlayHitSound();

            health.Health -= damage;

            navMeshAgent.enabled = false;
            rigidBody.isKinematic = false;

            Vector3 impulse = direction * force;
            rigidBody.AddForce(impulse, ForceMode.Impulse);

            character.State = CharacterState.Attacked;

            BackToNormalStateAfter(2.0f);
        }

        private void BackToNormalStateAfter(float delay)
        {
            if (backToNormalStateCoroutine != null)
            {
                StopCoroutine(backToNormalStateCoroutine);
                backToNormalStateCoroutine = null;
            }

            backToNormalStateCoroutine = StartCoroutine(BackToNormalStateCoroutine(delay));
        }

        private IEnumerator BackToNormalStateCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (character.State == CharacterState.Attacked)
            {
                navMeshAgent.enabled = true;
                rigidBody.isKinematic = true;

                character.State = CharacterState.Normal;
            }
        }
    }
}