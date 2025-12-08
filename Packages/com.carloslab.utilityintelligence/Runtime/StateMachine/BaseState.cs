// <copyright file="BaseState.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BaseState : BaseTask, IState
    {
        public abstract string Name { get; }

        public abstract bool CanGoToNextState { get; }

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

        void IState.Reset()
        {
            ResetState();
        }

        protected virtual void ResetState()
        {

        }

        protected virtual void OnEnter()
        {

        }

        protected virtual void OnExit()
        {

        }
    }
}