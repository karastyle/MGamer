// <copyright file="IUtilityIntelligenceComponent.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public interface IUtilityIntelligenceComponent
    {
        public Blackboard Blackboard { get; }
        UtilityAgent Agent { get; }
        IEntityFacade AgentFacade { get; }

        T GetComponent<T>();
        T GetComponentInChildren<T>();
    }
}