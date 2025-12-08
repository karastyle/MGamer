// <copyright file="ConsiderationListViewModelDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        ConsiderationListViewModelDecisionTab : NameListViewModel<DecisionModel, ConsiderationModel, ConsiderationItemViewModelDecisionTab>
    {

        #region ConsiderationListViewModelDecisionTab

        public override IReadOnlyList<ConsiderationModel> ItemModels => Model.Considerations;

        public ConsiderationListViewModel ConsiderationListViewModel => RootViewModel.ConsiderationListViewModel;


        public override bool Contains(string name)
        {
            return Model.HasConsideration(name);
        }

        protected override bool TryAddModelWithoutRecord(ConsiderationModel model, int index)
        {
            return Model.TryAddConsideration(index, model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(ConsiderationModel model)
        {
            return Model.TryRemoveConsideration(model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(string name, ConsiderationModel model)
        {
            return Model.TryRemoveConsideration(name, model);
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveConsideration(sourceIndex, destIndex);
        }

        #endregion

        #region ConsiderationListViewModel

        public event Action<ConsiderationItemViewModel> ConsiderationAdded;
        public event Action<ConsiderationItemViewModel> ConsiderationRemoved;
        public event Action<ConsiderationItemViewModel, string, string> ConsiderationNameChanged;


        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.ConsiderationListViewModel.ItemAdded -= ConsiderationListViewModel_OnItemAdded;
            viewModel.ConsiderationListViewModel.ItemRemoved -= ConsiderationListViewModel_OnItemRemoved;
            viewModel.ConsiderationListViewModel.ItemNameChanged -= ConsiderationListViewModel_OnItemNameChanged;
        }

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.ConsiderationListViewModel.ItemAdded += ConsiderationListViewModel_OnItemAdded;
            viewModel.ConsiderationListViewModel.ItemRemoved += ConsiderationListViewModel_OnItemRemoved;
            viewModel.ConsiderationListViewModel.ItemNameChanged += ConsiderationListViewModel_OnItemNameChanged;
        }

        private void ConsiderationListViewModel_OnItemAdded(ConsiderationItemViewModel item)
        {
            ConsiderationAdded?.Invoke(item);
        }

        private void ConsiderationListViewModel_OnItemRemoved(ConsiderationItemViewModel item)
        {
            TryRemoveItem(item.Name);
            ConsiderationRemoved?.Invoke(item);

            // Debug.Log($"ConsiderationListViewModelDecisionTab Decision: {Model.Name} Remove Item: {consideration.Name} Result: {result}");
        }

        private void ConsiderationListViewModel_OnItemNameChanged(ConsiderationItemViewModel item, string oldName, string newName)
        {
            Model.TryChangeConsiderationName(oldName, newName);
            ConsiderationNameChanged?.Invoke(item, oldName, newName);
        }

        #endregion

    }
}