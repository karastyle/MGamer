// <copyright file="ConsiderationItemViewModelIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationItemViewModelIntelligenceTab : BaseItemViewModel<ConsiderationModel, ConsiderationListViewModelIntelligenceTab>,
        INameViewModel, IStatusViewModel, INotifyBindablePropertyChanged
    {
        public event Action<InputItemViewModel> InputChanged;
        public event Action<InputNormalizationItemViewModel> InputNormalizationChanged;

        protected override void OnInit(ConsiderationModel model)
        {
            var considerationViewModel = RootViewModel.ConsiderationListViewModel.GetItemByName(model.Name);
            this.considerationViewModel = considerationViewModel;
            RegisterConsiderationViewModelEvents(considerationViewModel);

            contextViewModel.ConsiderationViewModel = considerationViewModel;
            contextViewModel.Init(model);
            RegisterContextViewModelEvents(contextViewModel);
        }

        protected override void OnDeinit()
        {
            UnregisterConsiderationViewModelEvents(considerationViewModel);
            considerationViewModel = null;

            UnregisterContextViewModelEvents(contextViewModel);
            contextViewModel.ConsiderationViewModel = null;
            contextViewModel.Deinit();
        }

        protected override void OnItemAdded()
        {
            decisionViewModel = ListViewModel.DecisionViewModel;
            contextViewModel.DecisionContextViewModel = decisionViewModel.ContextViewModel;
        }

        protected override void OnItemRemoved()
        {
            decisionViewModel = null;
            contextViewModel.DecisionContextViewModel = null;
        }

        #region DecisionViewModel

        private DecisionItemViewModelIntelligenceTab decisionViewModel;
        public DecisionItemViewModelIntelligenceTab DecisionViewModel => decisionViewModel;


        #endregion

        #region ConsiderationContextViewModel

        private ConsiderationContextViewModel contextViewModel = new();

        public ConsiderationContextViewModel ContextViewModel => contextViewModel;

        private void RegisterContextViewModelEvents(ConsiderationContextViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged += ConsiderationContextViewModel_PropertyChanged;
        }

        private void UnregisterContextViewModelEvents(ConsiderationContextViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged -= ConsiderationContextViewModel_PropertyChanged;
        }

        private void ConsiderationContextViewModel_PropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(CurrentStatus))
                StatusChanged?.Invoke(CurrentStatus);
        }

        #endregion

        #region ConsiderationItemViewModel

        public InputNormalizationItemViewModel InputNormalizationViewModel =>
            considerationViewModel?.InputNormalizationViewModel;

        private ConsiderationItemViewModel considerationViewModel;

        public ConsiderationItemViewModel ConsiderationViewModel => considerationViewModel;


        private void RegisterConsiderationViewModelEvents(ConsiderationItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.InputChanged += ConsiderationItemViewModel_OnInputChanged;
            viewModel.InputNormalizationChanged += ConsiderationItemViewModel_OnInputNormalizationChanged;
            viewModel.propertyChanged += ConsiderationItemViewModel_OnPropertyChanged;
        }

        private void UnregisterConsiderationViewModelEvents(ConsiderationItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.InputChanged -= ConsiderationItemViewModel_OnInputChanged;
            viewModel.InputNormalizationChanged -= ConsiderationItemViewModel_OnInputNormalizationChanged;
            viewModel.propertyChanged -= ConsiderationItemViewModel_OnPropertyChanged;
        }

        private void ConsiderationItemViewModel_OnInputChanged(InputItemViewModel viewModel)
        {
            InputChanged?.Invoke(viewModel);
        }

        private void ConsiderationItemViewModel_OnInputNormalizationChanged(InputNormalizationItemViewModel viewModel)
        {
            InputNormalizationChanged?.Invoke(viewModel);
        }

        private void ConsiderationItemViewModel_OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }

        #endregion

        #region ConsiderationItemViewModelIntelligenceTab

        [CreateProperty]
        public string Name
        {
            get => considerationViewModel.Name;
            set => considerationViewModel.Name = value;
        }

        [CreateProperty] public string InputName => considerationViewModel.InputName;

        [CreateProperty] public string InputNormalizationName => considerationViewModel.InputNormalizationName;

        public Type InputValueType => considerationViewModel.InputValueType;

        public InputItemViewModel Input => considerationViewModel?.InputViewModel;

        public InputNormalizationItemViewModel InputNormalization => considerationViewModel?.InputNormalizationViewModel;

        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            ContextViewModel.Model = newModel;
        }

        #endregion

        #region Status

        public event Action<Status> StatusChanged;

        public Status CurrentStatus => (Status)ContextViewModel.CurrentStatus;

        #endregion
    }
}