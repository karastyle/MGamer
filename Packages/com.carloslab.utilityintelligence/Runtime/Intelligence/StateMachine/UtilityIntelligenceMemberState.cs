// <copyright file="UtilityIntelligenceMemberState.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class UtilityIntelligenceMemberState : BaseState, IUtilityIntelligenceMember
    {
        #region IUtilityIntelligenceMember

        protected UtilityIntelligence rootObject;

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

        public bool IsEditorOpening => rootObject.IsEditorOpening;
        public bool IsRuntime => rootObject.IsRuntime;

        public UtilityIntelligence Intelligence => rootObject;
        public Blackboard Blackboard => rootObject.Blackboard;
        public UtilityAgent Agent => rootObject.Agent;
        public IEntityFacade AgentFacade => Agent.EntityFacade;

        public T GetComponent<T>()
        {
            return Agent.GetComponent<T>();
        }

        public T GetComponentInChildren<T>()
        {
            return Agent.GetComponentInChildren<T>();
        }

        protected virtual void OnRootObjectChanged(UtilityIntelligence intelligence)
        {

        }

        #endregion
    }
}