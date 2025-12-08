// <copyright file="BaseTask.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BaseTask : ITask
    {
        #region Status

        private Status currentStatus;
        private EndStatus endStatus;
        public EndStatus EndStatus => endStatus;
        public event Action<Status> StatusChanged;
        public Status CurrentStatus
        {
            get => currentStatus;
            private set
            {
                var oldStatus = currentStatus;
                var newStatus = value;

                if (oldStatus == newStatus)
                    return;

                currentStatus = newStatus;

                OnStatusChanged(oldStatus, newStatus);
                StatusChanged?.Invoke(currentStatus);
            }
        }

        public bool IsInactive => currentStatus == Status.Start || currentStatus == Status.End;

        public bool IsRunning => currentStatus == Status.Running;

        public bool IsEnd => currentStatus == Status.End;

        protected virtual void OnStatusChanged(Status oldStatus, Status newStatus)
        {

        }

        #endregion

        #region Execute

        internal ExecuteStatus Execute(float deltaTime)
        {
            Start();

            Update(deltaTime);

            End();

            return (ExecuteStatus)currentStatus;
        }

        private void Start()
        {
            if (currentStatus == Status.Start || currentStatus == Status.End)
            {
                OnStart();
                CurrentStatus = Status.Running;
            }
        }

        protected virtual void OnStart()
        {
        }

        private void Update(float deltaTime)
        {
            if (currentStatus == Status.Running)
            {
                CurrentStatus = (Status)OnUpdate(deltaTime);
            }
        }

        protected abstract UpdateStatus OnUpdate(float deltaTime);

        internal void End()
        {
            if (currentStatus == Status.Success
                || currentStatus == Status.Failure
                || currentStatus == Status.Aborted)
            {
                OnEnd();
                endStatus = (EndStatus)currentStatus;

                CurrentStatus = Status.End;
            }
        }

        //OnEnd is called after the action returns a success or failure
        protected virtual void OnEnd()
        {
        }

        #endregion

        #region ITask

        ExecuteStatus ITask.Execute(float deltaTime)
        {
            return Execute(deltaTime);
        }

        void ITask.Abort()
        {
            Abort();
        }

        void ITask.Reset()
        {
            ResetTask();
        }

        #endregion

        #region Other

        internal void Abort()
        {
            if (currentStatus == Status.Running)
            {
                OnAbort();
                CurrentStatus = Status.Aborted;
            }
        }

        //OnAbort is called when the action's target changes or when the agent makes a new decision
        protected virtual void OnAbort()
        {
        }

        internal void ResetTask()
        {
            CurrentStatus = Status.Start;
            endStatus = EndStatus.Start;
            // OnResetTask();
        }
        //
        // protected virtual void OnResetTask()
        // {
        //     
        // }

        #endregion

    }
}