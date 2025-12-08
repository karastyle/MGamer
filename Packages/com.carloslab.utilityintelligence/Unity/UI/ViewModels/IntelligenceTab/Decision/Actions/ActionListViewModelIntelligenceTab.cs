// <copyright file="ActionListViewModelIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionListViewModelIntelligenceTab : BaseListViewModel<DecisionModel, ActionModel, ActionItemViewModelIntelligenceTab>
    {
        public void Init(DecisionModel model, DecisionItemViewModelIntelligenceTab decisionViewModel)
        {
            this.decisionViewModel = decisionViewModel;
            Init(model);
        }

        #region ViewModels

        private DecisionItemViewModelIntelligenceTab decisionViewModel;
        public DecisionItemViewModelIntelligenceTab DecisionViewModel => decisionViewModel;

        public DecisionContextViewModel DecisionContextViewModel => decisionViewModel.ContextViewModel;

        #endregion

        #region ActionListViewModel

        private ActionListViewModel listViewModel;

        public ActionListViewModel ListViewModel
        {
            get => listViewModel;
            internal set
            {
                if (listViewModel == value)
                    return;

                UnregisterListViewModelEvents(listViewModel);
                listViewModel = value;
                RegisterListViewModelEvents(listViewModel);
            }
        }

        private void UnregisterListViewModelEvents(ActionListViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.ItemIndexChanged -= OnActionIndexChanged;
            viewModel.ItemAdded -= OnActionAdded;
            viewModel.ItemRemoved -= OnActionRemoved;
        }

        private void RegisterListViewModelEvents(ActionListViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.ItemIndexChanged += OnActionIndexChanged;
            viewModel.ItemAdded += OnActionAdded;
            viewModel.ItemRemoved += OnActionRemoved;
        }

        private void OnActionIndexChanged(int sourceIndex, int destIndex)
        {
            HandleItemIndexChanged(sourceIndex, destIndex);
        }

        private void OnActionAdded(ActionItemViewModel viewModel)
        {
            bool result = TryCreateItemWithoutModel(viewModel.Model, out _);

            // Debug.Log($"ConsiderationListViewModelIntelligenceTab Decision: {Model.Name} Add Item: {consideration.Name} Result: {result}");
        }

        private void OnActionRemoved(ActionItemViewModel itemViewModel)
        {
            bool result = TryRemoveItemWithoutModelById(itemViewModel.Model.Id, true);

            // Debug.Log($"ConsiderationListViewModelIntelligenceTab Decision: {Model.Name} Remove Item: {consideration.Name} Result: {result}");
        }

        #endregion

        #region Items

        public override IReadOnlyList<ActionModel> ItemModels => Model.Actions;


        protected override bool TryAddModelWithoutRecord(ActionModel model, int index)
        {
            return false;
        }

        protected override bool TryRemoveModelWithoutRecord(ActionModel model)
        {
            return false;
        }

        public override bool TryRenameItem(ActionItemViewModelIntelligenceTab item, string newName)
        {
            return false;
        }

        #endregion
    }
}