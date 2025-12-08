// <copyright file="Character.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.AI;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public enum Team
    {
        TeamA,
        TeamB,
        TeamC
    }

    public enum CharacterState
    {
        Normal,
        Attacked
    }

    public class Character : UtilityAgentFacade
    {
        [SerializeField]
        private Team team;
        public Team Team => team;

        private CharacterState state;

        public CharacterState State
        {
            get => state;
            set
            {
                var oldState = state;
                var newState = value;

                if (oldState == newState) return;

                state = newState;
                OnStateChanged(oldState, newState);
            }
        }

        private CharacterHealth health;
        public int Health => health.Health;

        private CharacterEnergy energy;
        public int Energy => energy.Energy;

        private Animator animator;
        private NavMeshAgent navMeshAgent;

        private void Awake()
        {
            health = GetComponent<CharacterHealth>();
            energy = GetComponent<CharacterEnergy>();

            animator = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            var blackboard = Entity.Intelligence.Blackboard;
            if (blackboard.TryGetVariable(BlackboardVariableNames.Animator, out AnimatorVariable animatorVariable))
                animatorVariable.Value = animator;

            if (blackboard.TryGetVariable(BlackboardVariableNames.NavMeshAgent, out NavMeshAgentVariable navMeshAgentVariable))
                navMeshAgentVariable.Value = navMeshAgent;
        }

        private void OnStateChanged(CharacterState oldState, CharacterState newState)
        {
            //Debug.Log($"Agent: {Entity.Name} BasicCharacter StateChanged OldState: {oldState} NewState: {newState} Frame: {Time.frameCount}");
        }
    }
}