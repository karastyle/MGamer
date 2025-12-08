// <copyright file="UtilityIntelligenceMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityIntelligenceMember : IUtilityIntelligenceMember
    {
        #region IUtilityIntelligenceMember

        private UtilityIntelligence rootObject;

        public UtilityIntelligence RootObject
        {
            get => rootObject;
            set
            {
                if (rootObject == value) return;

                rootObject = value;

                OnRootObjectChanged(rootObject);
            }
        }

        public bool IsEditorOpening => rootObject?.IsEditorOpening ?? false;
        public bool IsRuntime => rootObject?.IsRuntime ?? false;

        public UtilityIntelligence Intelligence => rootObject;
        public Blackboard Blackboard => rootObject?.Blackboard;
        public UtilityAgent Agent => rootObject?.Agent;
        public IEntityFacade AgentFacade => Agent?.EntityFacade;

        public T GetComponent<T>()
        {
            var agent = Agent;
            return agent != null ? agent.GetComponent<T>() : default;
        }

        public T GetComponentInChildren<T>()
        {
            var agent = Agent;
            return agent != null ? agent.GetComponentInChildren<T>() : default;
        }

        protected virtual void OnRootObjectChanged(UtilityIntelligence intelligence)
        {

        }

        #endregion

    }
}