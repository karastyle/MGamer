// <copyright file="UtilityAgent.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class UtilityAgent : UtilityEntity, IAgent
    {
        public UtilityAgent(UtilityIntelligence intelligence)
        {
            Intelligence = intelligence ?? new();
            Intelligence.Agent = this;
        }

        #region Properties
        public UtilityIntelligence Intelligence { get; internal set; }

        #endregion

        internal void MakeDecision(HashSet<UtilityEntity> entities)
        {
            Intelligence?.MakeDecision(entities);
        }

        internal void ExecuteDecision(float deltaTime)
        {
            Intelligence?.Execute(deltaTime);
        }

        internal void AbortDecision()
        {
            if (Intelligence == null) return;

            Intelligence.Abort();
            Intelligence.End();
        }
    }
}