// <copyright file="Parallel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Extensions;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Parallel : Composite
    {
        private readonly List<ExecuteStatus> childrenStatus = new();

        protected override void OnStart()
        {
            childrenStatus.Clear();
            for (int index = 0; index < ChildCount; index++)
            {
                childrenStatus.Add(ExecuteStatus.Start);
            }
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (ChildCount == 0)
                return UpdateStatus.Failure;

            var updateStatus = UpdateStatus.Success;

            bool exitLoop = false;
            for (int i = 0; i < ChildCount && !exitLoop; i++)
            {
                Task child = children[i];
                var executeStatus = childrenStatus[i];
                if (executeStatus != ExecuteStatus.End)
                {
                    executeStatus = child.Execute(deltaTime);
                    childrenStatus[i] = executeStatus;
                    switch (executeStatus)
                    {
                        case ExecuteStatus.Running:
                            updateStatus = UpdateStatus.Running;
                            break;
                        case ExecuteStatus.End:
                            var endStatus = child.EndStatus;
                            if (endStatus == EndStatus.Failure)
                            {
                                AbortChildren();
                                exitLoop = true;
                                updateStatus = UpdateStatus.Failure;
                            }

                            break;
                    }
                }
            }

            return updateStatus;
        }

        protected override void OnChildAdded(Task child)
        {
            childrenStatus.Add(ExecuteStatus.Running);
        }

        protected override void OnChildMoved(int sourceIndex, int destIndex)
        {
            childrenStatus.Move(sourceIndex, destIndex);
        }

        protected override void OnChildRemoved(int index)
        {
            childrenStatus.RemoveAt(index);
        }
    }
}