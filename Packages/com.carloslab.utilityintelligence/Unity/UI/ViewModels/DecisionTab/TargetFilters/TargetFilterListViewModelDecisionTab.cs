// <copyright file="TargetFilterListViewModelDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewModelDecisionTab : NameListViewModel<DecisionModel, TargetFilterModel, TargetFilterItemViewModelDecisionTab>
    {
        public override IReadOnlyList<TargetFilterModel> ItemModels => Model.TargetFilters;


        #region TargetFilterListViewModel

        public event Action<TargetFilterItemViewModel> TargetFilterAdded;
        public event Action<TargetFilterItemViewModel> TargetFilterRemoved;
        public event Action<TargetFilterItemViewModel, string, string> TargetFilterNameChanged;

        public TargetFilterListViewModel TargetFilterListViewModel => RootViewModel.TargetFilterListViewModel;

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.TargetFilterListViewModel.ItemAdded -= TargetFilterListViewModel_OnItemAdded;
            viewModel.TargetFilterListViewModel.ItemRemoved -= TargetFilterListViewModel_OnItemRemoved;
            viewModel.TargetFilterListViewModel.ItemNameChanged -= TargetFilterListViewModel_OnItemNameChanged;
        }

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.TargetFilterListViewModel.ItemAdded += TargetFilterListViewModel_OnItemAdded;
            viewModel.TargetFilterListViewModel.ItemRemoved += TargetFilterListViewModel_OnItemRemoved;
            viewModel.TargetFilterListViewModel.ItemNameChanged += TargetFilterListViewModel_OnItemNameChanged;
        }

        private void TargetFilterListViewModel_OnItemAdded(TargetFilterItemViewModel item)
        {
            TargetFilterAdded?.Invoke(item);
        }

        private void TargetFilterListViewModel_OnItemRemoved(TargetFilterItemViewModel item)
        {
            TryRemoveItem(item.Name);
            TargetFilterRemoved?.Invoke(item);
        }

        private void TargetFilterListViewModel_OnItemNameChanged(TargetFilterItemViewModel item, string oldName, string newName)
        {
            Model.TryChangeTargetFilterName(oldName, newName);
            TargetFilterNameChanged?.Invoke(item, oldName, newName);
        }
        #endregion

        public override bool Contains(string name)
        {
            return Model.HasTargetFilter(name);
        }

        protected override bool TryAddModelWithoutRecord(TargetFilterModel model, int index)
        {
            Model.TryAddTargetFilter(index, model.Name, model);
            return true;
        }

        protected override bool TryRemoveModelWithoutRecord(TargetFilterModel model)
        {
            Model.TryRemoveTargetFilter(model.Name, model);
            return true;
        }

        protected override bool TryRemoveModelWithoutRecord(string name, TargetFilterModel model)
        {
            return Model.TryRemoveTargetFilter(name, model);
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveTargetFilter(sourceIndex, destIndex);
        }


    }
}