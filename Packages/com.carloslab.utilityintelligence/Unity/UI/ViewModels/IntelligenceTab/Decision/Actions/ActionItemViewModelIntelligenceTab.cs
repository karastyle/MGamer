// <copyright file="ActionItemViewModelIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionItemViewModelIntelligenceTab : BaseItemViewModel<ActionModel, ActionListViewModelIntelligenceTab>, ITypeNameViewModel, IStatusViewModel
    {
        protected override void OnItemAdded()
        {
            var actionViewModel = ListViewModel.ListViewModel.GetItemByModelId(Model.Id);
            this.actionViewModel = actionViewModel;
            RegisterActionViewModelEvents(actionViewModel);

            decisionContextViewModel = ListViewModel.DecisionContextViewModel;
        }

        protected override void OnItemRemoved()
        {
            UnregisterActionViewModelEvents(actionViewModel);
            actionViewModel = null;
            decisionContextViewModel = null;
        }

        public string TypeName => Model?.RuntimeType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;

        #region DecisionViewModel

        private DecisionContextViewModel decisionContextViewModel;

        #endregion

        #region ActionViewModel

        private ActionItemViewModel actionViewModel;


        private void RegisterActionViewModelEvents(ActionItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged += ActionViewModel_OnPropertyChanged;
        }

        private void UnregisterActionViewModelEvents(ActionItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged -= ActionViewModel_OnPropertyChanged;
        }

        private void ActionViewModel_OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }

        #endregion

        #region Status

        public Status CurrentStatus
        {
            get
            {
                if (decisionContextViewModel.IsMatchContext)
                    return Model.Runtime.CurrentStatus;

                return Status.Start;
            }
        }

        public event Action<Status> StatusChanged;

        protected override void OnRegisterModelEvents(ActionModel model)
        {
            if (IsRuntime)
                model.Runtime.StatusChanged += OnStatusChanged;
        }

        protected override void OnUnregisterModelEvents(ActionModel model)
        {
            model.Runtime.StatusChanged -= OnStatusChanged;
        }

        private void OnStatusChanged(Status newStatus)
        {
            if (decisionContextViewModel.IsMatchContext)
                StatusChanged?.Invoke(newStatus);
            else
                StatusChanged?.Invoke(Status.Start);
        }

        #endregion

    }
}