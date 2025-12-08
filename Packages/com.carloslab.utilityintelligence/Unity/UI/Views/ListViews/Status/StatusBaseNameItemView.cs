// <copyright file="StatusBaseNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class StatusBaseNameItemView<TItemViewModel> : BaseNameItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel, IStatusViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        private const string RunningClassName = "running";
        private const string FailureClassName = "failure";
        private const string AbortedClassName = "aborted";
        private const string SuccessClassName = "success";

        private readonly bool enableStatus;

        protected StatusBaseNameItemView(bool enableStatus, bool enableRename, bool enableRemove) : base(enableRename, enableRemove)
        {
            this.enableStatus = enableStatus;
        }
        protected override void OnRegisterViewModelEvents(TItemViewModel viewModel)
        {
            if (enableStatus)
            {
                // if (viewModel.ModelObject is ConsiderationModel considerationModel && viewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
                // {
                //     var runtime = considerationModel.Runtime;
                //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} CurrentStatus: {currentStatus} NewStatus: {considerationViewModel.CurrentStatus}");
                // }

                // if (viewModel.ModelObject is DecisionModel model)
                // {
                //     var runtime = model.Runtime;
                //     string bestDecisionName = runtime.Intelligence.Context.BestDecision?.Name;
                //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {runtime.Name} BestDecision: {bestDecisionName} CurrentStatus: {currentStatus} NewStatus: {viewModel.CurrentStatus}");
                // }

                UpdateStatus(viewModel.CurrentStatus, true);
                viewModel.StatusChanged += OnStatusChanged;
            }
        }

        protected override void OnUnregisterViewModelEvents(TItemViewModel viewModel)
        {
            if (enableStatus)
            {
                viewModel.StatusChanged -= OnStatusChanged;
                // UpdateStatus(Status.Start, true);
            }
        }

#if UNITY_EDITOR
        protected override void OnEnterEditMode()
        {
            if (enableStatus)
            {
                ViewModel.StatusChanged -= OnStatusChanged;
                CurrentStatus = Status.Start;
            }
        }
#endif

        private void EnableStatus(Status status)
        {
            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} EnableStatus: {status}");
            // }

            // if (ViewModel.ModelObject is DecisionModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"StatusBaseNameItemView Agent: {runtime.Intelligence.AgentName} Decision: {runtime.Name} EnableStatus Status: {status}");
            // }

            string className = ConvertStatusToClass(status);
            if (string.IsNullOrEmpty(className)) return;

            EnableStatusClass(className);
        }

        private void DisableStatus(Status status, bool immediate)
        {
            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} DisableStatus: {status}");
            // }

            // if (ViewModel.ModelObject is DecisionModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"StatusBaseNameItemView Agent: {runtime.Intelligence.AgentName} Decision: {runtime.Name} DisableStatus Status: {status}");
            // }
            string className = ConvertStatusToClass(status);
            if (string.IsNullOrEmpty(className)) return;

            if (immediate)
                DisableStatusClass(className);
            else
                DisableStatusClassAfter(className, 100);
        }

        protected void EnableStatusClass(string stateClassName)
        {
            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} EnableStatusClass: {stateClassName}");
            // }
            ItemContainerView?.AddToClassList(stateClassName);
        }

        protected void EnableStatusClassAfter(string stateClassName, long delayTime)
        {
            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} EnableStatusClassAfter: {stateClassName}");
            // }

            schedule.Execute(() => EnableStatusClass(stateClassName)).StartingIn(delayTime);
        }

        protected void DisableStatusClass(string statusClassName)
        {
            if (ItemContainerView == null) return;

            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} DisableStatusClass: {statusClassName}");
            // }


            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     
            //     var classes = ItemContainerView.GetClasses();
            //     var classNames = classes.ToArray();
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} DisableStatusClass: {statusClassName} ClassCount: {classNames.Length}");
            //     foreach (var className in classNames)
            //     {
            //         Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} DisableStatusClass: {statusClassName} ClassName: {className}");
            //     }
            // }
            ItemContainerView.RemoveFromClassList(statusClassName);
        }

        protected void DisableStatusClassAfter(string statusClassName, long delayTime)
        {
            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} DisableStatusClassAfter: {statusClassName}");
            // }

            schedule.Execute(() =>
            {
                Status status = ConvertClassToStatus(statusClassName);
                if (status == currentStatus) return;

                DisableStatusClass(statusClassName);

            }).StartingIn(delayTime);
        }

        #region Status

        private Status currentStatus;

        public Status CurrentStatus
        {
            get => currentStatus;
            set
            {
                Status oldStatus = currentStatus;
                Status newStatus = value;

                if (oldStatus != Status.Running)
                    UpdateStatus(newStatus, false);
                else
                    UpdateStatus(newStatus, true);
            }
        }

        private Status ConvertClassToStatus(string className)
        {
            Status status = Status.Start;
            switch (className)
            {
                case RunningClassName:
                    status = Status.Running;
                    break;
                case SuccessClassName:
                    status = Status.Success;
                    break;
                case FailureClassName:
                    status = Status.Failure;
                    break;
                case AbortedClassName:
                    status = Status.Aborted;
                    break;
                default:
                    break;
            }

            return status;
        }

        private string ConvertStatusToClass(Status status)
        {
            string className = null;
            switch (status)
            {
                case Status.Running:
                    className = RunningClassName;
                    break;
                case Status.Success:
                    className = SuccessClassName;
                    break;
                case Status.Failure:
                    className = FailureClassName;
                    break;
                case Status.Aborted:
                    className = AbortedClassName;
                    break;
            }

            return className;
        }

        private void UpdateStatus(Status newStatus, bool immediate)
        {
            if (currentStatus == newStatus)
                return;

            // if (ViewModel.ModelObject is ConsiderationModel considerationModel && ViewModel is ConsiderationItemViewModelIntelligenceTab considerationViewModel)
            // {
            //     var runtime = considerationModel.Runtime;
            //     Debug.Log($"Agent: {runtime.Intelligence.AgentName} Decision: {considerationViewModel.DecisionViewModel.Name} Consideration: {considerationViewModel.Name} StatusChanged From: {currentStatus} To: {newStatus}");
            // }

            DisableStatus(currentStatus, immediate);
            currentStatus = newStatus;
            EnableStatus(currentStatus);
        }

        private void OnStatusChanged(Status newStatus)
        {
            CurrentStatus = newStatus;
        }

        #endregion
    }
}