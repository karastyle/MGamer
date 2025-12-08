// <copyright file="ConsiderationListViewModelIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationListViewModelIntelligenceTab : NameListViewModel<DecisionModel, ConsiderationModel, ConsiderationItemViewModelIntelligenceTab>
    {
        public void Init(DecisionModel model, DecisionItemViewModelIntelligenceTab decisionViewModel)
        {
            this.decisionViewModel = decisionViewModel;
            base.Init(model);
        }

        protected override void OnDeinit()
        {
            this.decisionViewModel = null;
        }

        #region ViewModels

        public ConsiderationListViewModel ListViewModel => RootViewModel.ConsiderationListViewModel;

        private DecisionItemViewModelIntelligenceTab decisionViewModel;
        public DecisionItemViewModelIntelligenceTab DecisionViewModel => decisionViewModel;

        #endregion

        #region ConsiderationListViewModelIntelligenceTab

        public override IReadOnlyList<ConsiderationModel> ItemModels => Model.Considerations;


        public override bool Contains(string name)
        {
            return Model.HasConsideration(name);
        }

        protected override bool TryAddModelWithoutRecord(ConsiderationModel model, int index)
        {
            return false;
        }

        protected override bool TryRemoveModelWithoutRecord(ConsiderationModel model)
        {
            return false;
        }

        protected override bool TryRemoveModelWithoutRecord(string name, ConsiderationModel model)
        {
            return false;
        }

        #endregion

        #region ConsiderationListViewModelDecisionTab

        private ConsiderationListViewModelDecisionTab listViewModelDecisionTab;

        public ConsiderationListViewModelDecisionTab ListViewModelDecisionTab
        {
            get => listViewModelDecisionTab;
            internal set
            {
                if (listViewModelDecisionTab == value)
                    return;

                if (listViewModelDecisionTab != null) UnregisterListViewModelDecisionTabEvents(listViewModelDecisionTab);
                listViewModelDecisionTab = value;
                if (listViewModelDecisionTab != null) RegisterListViewModelDecisionTabEvents(listViewModelDecisionTab);
            }
        }

        private void UnregisterListViewModelDecisionTabEvents(ConsiderationListViewModelDecisionTab viewModel)
        {
            viewModel.ItemIndexChanged -= OnConsiderationIndexChanged;
            viewModel.ItemAdded -= OnConsiderationAdded;
            viewModel.ItemRemoved -= OnConsiderationRemoved;
        }

        private void RegisterListViewModelDecisionTabEvents(ConsiderationListViewModelDecisionTab viewModel)
        {
            viewModel.ItemIndexChanged += OnConsiderationIndexChanged;
            viewModel.ItemAdded += OnConsiderationAdded;
            viewModel.ItemRemoved += OnConsiderationRemoved;
        }

        private void OnConsiderationIndexChanged(int sourceIndex, int destIndex)
        {
            HandleItemIndexChanged(sourceIndex, destIndex);
        }

        private void OnConsiderationAdded(ConsiderationItemViewModelDecisionTab consideration)
        {
            bool result = TryCreateItemWithoutModel(consideration.Model, out _);

            // Debug.Log($"ConsiderationListViewModelIntelligenceTab Decision: {Model.Name} Add Item: {consideration.Name} Result: {result}");
        }

        private void OnConsiderationRemoved(ConsiderationItemViewModelDecisionTab consideration)
        {
            bool result = TryRemoveItemWithoutModel(consideration.Name);

            // Debug.Log($"ConsiderationListViewModelIntelligenceTab Decision: {Model.Name} Remove Item: {consideration.Name} Result: {result}");
        }

        #endregion

    }
}