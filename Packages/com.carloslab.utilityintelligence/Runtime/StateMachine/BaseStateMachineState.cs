// <copyright file="BaseStateMachineState.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BaseStateMachineState<TState> : BaseStateMachine<TState>, IState
        where TState : BaseTask, IState
    {
        public abstract string Name { get; }

        void IState.Enter()
        {
            if (CurrentStatus == Status.Start)
            {
                OnEnter();
            }
        }

        void IState.Exit()
        {
            if (CurrentStatus == Status.End)
            {
                ResetTask();

                OnExit();
            }
        }

        protected virtual void ResetState()
        {

        }

        void IState.Reset()
        {
            State = null;
            nextState = null;
            nextStateUpdated = false;
            ResetState();
        }

        protected virtual void OnEnter()
        {

        }

        protected virtual void OnExit()
        {

        }
    }
}