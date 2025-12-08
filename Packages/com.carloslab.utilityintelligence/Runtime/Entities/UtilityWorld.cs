// <copyright file="UtilityWorld.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class UtilityWorld : World<UtilityWorld, UtilityEntity>
    {
        private float decisionMakingInterval;
        private float decisionMakingElapsedTime;

        private DecisionMakingBatchProcessor decisionMakingBatchProcessor = new();

        private HashSet<UtilityAgent> enabledAgents = new();

        private bool enableDecisionMakingBatchProcessing;

        public bool EnableDecisionMakingBatchProcessing
        {
            get => enableDecisionMakingBatchProcessing;
            set => enableDecisionMakingBatchProcessing = value;
        }

        public int DecisionMakingBatchSize
        {
            get => decisionMakingBatchProcessor.BatchSize;
            set => decisionMakingBatchProcessor.BatchSize = value;
        }

        #region Properties

        public float DecisionMakingInterval
        {
            get => decisionMakingInterval;
            internal set => decisionMakingInterval = value;
        }


        public IReadOnlyCollection<UtilityAgent> EnabledAgents => enabledAgents;


        #endregion

        #region Event Functions

        protected override void OnStart()
        {
            decisionMakingElapsedTime = decisionMakingInterval;
        }

        protected override void OnUpdate(float deltaTime)
        {
            // UtilityIntelligenceConsole.Instance.Log($"OnUpdate Tick: {UpdateTick} Start");

            decisionMakingElapsedTime += deltaTime;
            if (decisionMakingElapsedTime >= decisionMakingInterval)
            {
                if (enableDecisionMakingBatchProcessing)
                {
                    if (!MakeDecisionsInBatches())
                    {
                        decisionMakingElapsedTime = 0;
                    }
                }
                else
                {
                    MakeDecisions();
                    decisionMakingElapsedTime = 0;
                }
            }

            ExecuteDecisions(deltaTime);

            // UtilityIntelligenceConsole.Instance.Log($"OnUpdate Tick: {UpdateTick} End");
        }

        protected override void OnEntityEnabled(UtilityEntity entity)
        {
            if (entity is UtilityAgent agent)
            {
                OnAgentEnabled(agent);
            }
        }

        private void OnAgentEnabled(UtilityAgent agent)
        {
            WorldConsole.Instance.Log($"OnAgentEnabled: {agent.Name} Frame: {WorldTime.Instance.Frame}");
            enabledAgents.Add(agent);
        }

        protected override void OnEntityDisabled(UtilityEntity entity)
        {
            if (entity is UtilityAgent agent)
            {
                OnAgentDisabled(agent);
            }
        }

        private void OnAgentDisabled(UtilityAgent agent)
        {
            WorldConsole.Instance.Log($"OnAgentDisabled: {agent.Name} Frame: {WorldTime.Instance.Frame}");
            agent.AbortDecision();
            enabledAgents.Remove(agent);
        }

        #endregion

        #region Decisions

        private bool MakeDecisionsInBatches()
        {
#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("UtilityWorld - MakeDecisionsInBatches");
#endif

            if (decisionMakingBatchProcessor.AgentCount == 0)
                decisionMakingBatchProcessor.AddAgents(enabledAgents);

            bool isRunning = decisionMakingBatchProcessor.MakeDecisions(enabledEntities);
            return isRunning;
        }

        private void MakeDecisions()
        {
#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("UtilityWorld - MakeDecisions");
#endif
            foreach (UtilityAgent agent in enabledAgents)
            {
                agent.MakeDecision(enabledEntities);
            }
        }

        private void ExecuteDecisions(float deltaTime)
        {
#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("UtilityWorld - ExecuteDecisions");
#endif

            foreach (UtilityAgent agent in enabledAgents)
            {
                agent.ExecuteDecision(deltaTime);
            }
        }

        #endregion
    }
}