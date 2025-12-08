// <copyright file="Decision.State.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision
    {
        private bool keepRunningUntilFinished;

        public bool KeepRunningUntilFinished
        {
            get => keepRunningUntilFinished;
            internal set => keepRunningUntilFinished = value;
        }

        public override bool CanGoToNextState
        {
            get
            {
                if (keepRunningUntilFinished)
                    return IsEnd;
                else
                    return true;
            }
        }

        protected override void ResetState()
        {
            ClearContexts();
        }

        protected override void OnEnter()
        {
            StateMachineConsole.Instance.Log(
                $"Agent: {Intelligence.AgentName} Decision: {Name} OnEnter Frame: {WorldTime.Instance.Frame}");
            Context = nextContext;
        }



        protected override void OnExit()
        {
            StateMachineConsole.Instance.Log(
                $"Agent: {Intelligence.AgentName} Decision: {Name} OnExit Frame: {WorldTime.Instance.Frame}");
        }

        // protected override void OnStatusChanged(Status oldStatus, Status newStatus)
        // {
        //     StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnStatusChanged OldStatus: {oldStatus} NewStatus: {newStatus} Frame: {FrameInfo.Frame}");
        // }
    }
}