// <copyright file="ConsiderationItemViewModelDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationItemViewModelDecisionTab : BaseItemViewModel<ConsiderationModel, ConsiderationListViewModelDecisionTab>,
        INameViewModel, INotifyBindablePropertyChanged
    {
        protected override void OnInit(ConsiderationModel model)
        {
            ConsiderationItemViewModel viewModel = RootViewModel.ConsiderationListViewModel.GetItemByName(model.Name);
            considerationViewModel = viewModel;
            RegisterConsiderationViewModelEvents(considerationViewModel);
        }

        protected override void OnDeinit()
        {
            UnregisterConsiderationViewModelEvents(considerationViewModel);
            considerationViewModel = null;
        }


        #region ConsiderationItemViewModel

        private ConsiderationItemViewModel considerationViewModel;

        public ConsiderationItemViewModel ConsiderationViewModel => considerationViewModel;


        public event Action<InputNormalizationItemViewModel> InputNormalizationChanged;

        private void RegisterConsiderationViewModelEvents(ConsiderationItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.InputNormalizationChanged += ConsiderationItemViewModel_OnInputNormalizationChanged;
            viewModel.propertyChanged += ConsiderationItemViewModel_OnPropertyChanged;
        }

        private void UnregisterConsiderationViewModelEvents(ConsiderationItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.InputNormalizationChanged -= ConsiderationItemViewModel_OnInputNormalizationChanged;
            viewModel.propertyChanged -= ConsiderationItemViewModel_OnPropertyChanged;
        }

        private void ConsiderationItemViewModel_OnInputNormalizationChanged(InputNormalizationItemViewModel item)
        {
            InputNormalizationChanged?.Invoke(item);
        }

        private void ConsiderationItemViewModel_OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }

        #endregion

        #region ConsiderationItemViewModelDecisionTab

        [CreateProperty]
        public string Name
        {
            get => considerationViewModel.Name;
            set => considerationViewModel.Name = value;
        }

        [CreateProperty] public string InputName => considerationViewModel.InputName;

        [CreateProperty] public string InputNormalizationName => considerationViewModel.InputNormalizationName;

        public Type InputValueType => considerationViewModel.InputValueType;

        public InputItemViewModel Input => considerationViewModel.InputViewModel;
        public InputNormalizationItemViewModel InputNormalization => considerationViewModel.InputNormalizationViewModel;

        #endregion

    }
}