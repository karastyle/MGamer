// <copyright file="Sequencer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Sequencer : Composite
    {
        private int currentIndex;

        protected override void OnStart()
        {
            currentIndex = 0;
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Sequencer OnStart Frame: {WorldTime.Instance.Frame}");
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var updateStatus = UpdateStatus.Failure;

            if (ChildCount == 0)
                return updateStatus;

            if (currentIndex < ChildCount)
            {
                Task child = children[currentIndex];

                TaskConsole.Instance.Log($"Agent: {Agent.Name} Sequencer OnUpdate childName: {child.GetType().Name} Frame: {WorldTime.Instance.Frame}");

                var executeStatus = child.Execute(deltaTime);

                switch (executeStatus)
                {
                    case ExecuteStatus.Running:
                        updateStatus = UpdateStatus.Running;
                        break;
                    case ExecuteStatus.End:
                        var endStatus = child.EndStatus;
                        switch (endStatus)
                        {
                            case EndStatus.Failure:
                                updateStatus = UpdateStatus.Failure;
                                break;
                            case EndStatus.Aborted:
                            case EndStatus.Success:
                                if (currentIndex < ChildCount - 1)
                                {
                                    currentIndex++;
                                    updateStatus = UpdateStatus.Running;
                                }
                                else
                                {
                                    updateStatus = UpdateStatus.Success;
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return updateStatus;
        }

        protected override void OnAbort()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Sequencer OnEnd Frame: {WorldTime.Instance.Frame}");
            base.OnAbort();
        }

        protected override void OnEnd()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Sequencer OnEnd Frame: {WorldTime.Instance.Frame}");

            base.OnEnd();
        }
    }
}