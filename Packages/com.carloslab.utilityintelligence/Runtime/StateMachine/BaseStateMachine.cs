// <copyright file="BaseStateMachine.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BaseStateMachine<TState> : BaseTask
        where TState : BaseTask, IState
    {
        public abstract string AgentName { get; }
        public virtual bool IsRuntime { get; internal set; }

        public event Action<TState> StateChanged;

        protected TState state;

        public TState State
        {
            get => state;
            internal set
            {
                var oldState = state;
                var newState = value;

                if (oldState != null)
                {
                    oldState.Exit();
                    OnUnregisterStateEvents(oldState);
                }

                state = newState;

                if (newState != null)
                {
                    OnRegisterStateEvents(newState);
                    newState.Enter();
                }

                HandleStateChanged(oldState, newState);
                StateChanged?.Invoke(newState);
            }
        }

        protected bool nextStateUpdated;
        protected TState nextState;
        public TState NextState
        {
            get => nextState;
            set
            {
                nextStateUpdated = true;

                nextState = value;

                // if (this is DecisionMaker decisionMaker)
                // {
                //     StateMachineConsole.Instance.Log($"Agent: {decisionMaker.AgentName} DecisionMaker Set NextState CurrentState: {state?.Name} CurrentStateStatus: {state?.CurrentStatus} NextState: {nextState?.Name} Frame: {FrameInfo.Frame}");
                // }

                if (state != nextState)
                {
                    if (state is { CanGoToNextState: true, IsRunning: true })
                    {
                        state.Abort();
                        state.End();
                    }
                }
            }
        }

        public bool CanTransitionToNextState
        {
            get
            {
                if (nextStateUpdated && CanGoToNextState)
                    return true;

                return false;
            }
        }

        private bool TryTransitionToNextState()
        {
            if (!CanTransitionToNextState) return false;

            if (state == null)
            {
                State = nextState;

                nextStateUpdated = false;
                return true;
            }
            else if (state.CanGoToNextState)
            {
                if (state.IsRunning)
                {
                    state.Abort();
                    state.End();
                }

                State = nextState;

                nextStateUpdated = false;
                return true;
            }

            return false;
        }

        public bool CanGoToNextState
        {
            get
            {
                if (state == null || state.CanGoToNextState)
                    return true;

                return false;
            }
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var updateStatus = UpdateStatus.Failure;

            if (state == null || state.IsEnd)
            {
                // if (this is DecisionMaker decisionMaker)
                // {
                //     StateMachineConsole.Instance.Log($"Agent: {decisionMaker.AgentName} DecisionMaker CurrentState: {state?.Name} NextState: {nextState?.Name} Frame: {FrameInfo.Frame}");
                // }

                if (!TryTransitionToNextState())
                {
                    updateStatus = UpdateStatus.Running;
                    return updateStatus;
                }

                if (state == null)
                {
                    updateStatus = UpdateStatus.Failure;
                    return updateStatus;
                }
            }

            var executeStatus = state.Execute(deltaTime);
            switch (executeStatus)
            {
                case ExecuteStatus.Running:
                    updateStatus = UpdateStatus.Running;
                    break;
                case ExecuteStatus.End:
                    var endStatus = state.EndStatus;
                    switch (endStatus)
                    {
                        case EndStatus.Success:
                            updateStatus = UpdateStatus.Success;
                            break;
                        case EndStatus.Failure:
                            updateStatus = UpdateStatus.Failure;
                            break;
                    }
                    break;
            }

            return updateStatus;
        }

        protected void HandleStateChanged(TState oldState, TState newState)
        {
            if (oldState != newState)
                oldState?.Reset();

            OnStateChanged(oldState, newState);
        }

        protected virtual void OnStateChanged(TState oldState, TState newState)
        {

        }

        protected virtual void OnRegisterStateEvents(TState state)
        {

        }

        protected virtual void OnUnregisterStateEvents(TState state)
        {

        }

        protected override void OnAbort()
        {
            state?.Abort();
        }

        protected override void OnEnd()
        {
            state?.End();
        }
    }
}