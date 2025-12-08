// <copyright file="Decision.Task.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision
    {
        private Repeater task;

        public Repeater Task => task;

        protected override void OnAbort()
        {
            StateMachineConsole.Instance.Log(
                $"Agent: {Intelligence.AgentName} Decision: {Name} OnAbort Frame: {WorldTime.Instance.Frame}");
            task.Abort();
        }

        /*
        protected override void OnStart()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnStart Frame: {WorldTime.Instance.Frame}");
        }
        */

        protected override void OnEnd()
        {
            StateMachineConsole.Instance.Log(
                $"Agent: {Intelligence.AgentName} Decision: {Name} OnEnd Frame: {WorldTime.Instance.Frame}");
            task.End();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var updateStatus = UpdateStatus.Failure;

            var target = Context.Target;

            // UtilityIntelligenceConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnUpdate Target: {target?.Name} IsTargetEnabled: {target?.IsEnabled} IsTargetDestroyed: {target?.IsDestroyed} Frame: {WorldTime.Instance.Frame}");

            if (!hasNoTarget && (target == null || target.IsEnabled == false || target.IsDestroyed))
            {
                task.Abort();
                return updateStatus;
            }

            var executeStatus = task.Execute(deltaTime);

            switch (executeStatus)
            {
                case ExecuteStatus.Running:
                    updateStatus = UpdateStatus.Running;
                    break;
                case ExecuteStatus.End:
                    var endStatus = task.EndStatus;
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
    }
}