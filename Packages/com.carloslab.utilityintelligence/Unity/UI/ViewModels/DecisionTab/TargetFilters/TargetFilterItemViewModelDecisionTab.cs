// <copyright file="TargetFilterItemViewModelDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemViewModelDecisionTab : BaseItemViewModel<TargetFilterModel, TargetFilterListViewModelDecisionTab>, INameViewModel, INotifyBindablePropertyChanged
    {
        protected override void OnInit(TargetFilterModel model)
        {
            var viewModel = RootViewModel.TargetFilterListViewModel.GetItemByName(model.Name);
            targetFilterViewModel = viewModel;
            RegisterTargetFilterViewModelEvents(targetFilterViewModel);
        }

        protected override void OnDeinit()
        {
            UnregisterTargetFilterViewModelEvents(targetFilterViewModel);
            targetFilterViewModel = null;
        }

        [CreateProperty]
        public string Name
        {
            get => targetFilterViewModel.Name;
            set => targetFilterViewModel.Name = value;
        }

        private TargetFilterItemViewModel targetFilterViewModel;
        public TargetFilterItemViewModel TargetFilterViewModel => targetFilterViewModel;

        private void RegisterTargetFilterViewModelEvents(TargetFilterItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged += TargetFilterViewModel_OnPropertyChanged;
        }

        private void UnregisterTargetFilterViewModelEvents(TargetFilterItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged -= TargetFilterViewModel_OnPropertyChanged;
        }

        private void TargetFilterViewModel_OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }
    }
}