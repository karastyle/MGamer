// <copyright file="UtilityIntelligenceMemberStateMachineState.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class UtilityIntelligenceMemberStateMachineState<TState> : BaseStateMachineState<TState>, IUtilityIntelligenceMember
        where TState : BaseTask, IState
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
        public override bool IsRuntime => rootObject?.IsRuntime ?? false;

        public UtilityIntelligence Intelligence => rootObject;

        public Blackboard Blackboard => rootObject?.Blackboard;

        public UtilityAgent Agent => rootObject?.Agent;
        public IEntityFacade AgentFacade => Agent?.EntityFacade;

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