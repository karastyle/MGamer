// <copyright file="DecisionMakingBatchProcessor.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public class DecisionMakingBatchProcessor
    {
        private int batchSize;
        public int BatchSize { get => batchSize; set => batchSize = value; }

        private List<UtilityAgent> agents = new();
        private int currentBatchIndex;

        public int AgentCount => agents.Count;

        private bool isRunning = false;
        public bool IsRunning => isRunning;


        public DecisionMakingBatchProcessor(int batchSize = 40)
        {
            this.BatchSize = batchSize;
        }

        public void AddAgents(HashSet<UtilityAgent> agents)
        {
            foreach (var agent in agents)
            {
                this.agents.Add(agent);
            }
        }

        public bool MakeDecisions(HashSet<UtilityEntity> entities)
        {
            int startIndex = currentBatchIndex * batchSize;
            int endIndex = Math.Min(startIndex + batchSize, agents.Count);

            if (startIndex < endIndex)
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    var agent = agents[i];
                    if (agent.IsEnabled)
                        agent.MakeDecision(entities);
                }

                currentBatchIndex++;
                isRunning = true;
            }
            else
            {
                currentBatchIndex = 0;
                agents.Clear();
                isRunning = false;
            }

            return isRunning;
        }
    }
}