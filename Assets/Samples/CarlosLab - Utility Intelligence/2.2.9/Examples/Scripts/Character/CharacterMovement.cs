// <copyright file="CharacterMovement.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;
using UnityEngine.AI;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField]
        private float stepInterval = 0.3f;

        private float stepTimer;

        private NavMeshAgent navMeshAgent;
        private CharacterSoundPlayer soundPlayer;

        public bool IsMoving
        {
            get
            {
                bool isMoving = navMeshAgent.enabled
                                && navMeshAgent.isStopped == false
                                && navMeshAgent.velocity.magnitude > 0.1f;
                return isMoving;
            }
        }

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            soundPlayer = GetComponentInChildren<CharacterSoundPlayer>();
        }

        private void Update()
        {
            if (IsMoving)
            {
                stepTimer -= Time.deltaTime;

                if (stepTimer <= 0f)
                {
                    soundPlayer.PlayFootstepSound();
                    stepTimer = stepInterval;
                }
            }
            else
            {
                stepTimer = 0.0f;
            }
        }
    }
}