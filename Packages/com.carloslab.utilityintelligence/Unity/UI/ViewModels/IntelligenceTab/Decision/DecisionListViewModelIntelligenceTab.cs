// <copyright file="DecisionListViewModelIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionListViewModelIntelligenceTab : NameListViewModel<DecisionMakerModel, DecisionModel, DecisionItemViewModelIntelligenceTab>
    {
        public void Init(DecisionMakerModel model, DecisionMakerItemViewModel decisionMaker)
        {
            this.decisionMakerViewModel = decisionMaker;
            Init(model);
        }

        #region DecisionMakerItemViewModel

        private DecisionMakerItemViewModel decisionMakerViewModel;
        public DecisionMakerItemViewModel DecisionMakerViewModel => decisionMakerViewModel;

        #endregion

        #region DecisionListViewModel

        public event Action<DecisionItemViewModel> DecisionAdded;
        public event Action<DecisionItemViewModel> DecisionRemoved;
        public event Action<DecisionItemViewModel, string, string> DecisionNameChanged;


        public DecisionListViewModel DecisionListViewModel => RootViewModel.DecisionListViewModel;

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.DecisionListViewModel.ItemAdded -= DecisionListViewModel_OnItemAdded;
            viewModel.DecisionListViewModel.ItemRemoved -= DecisionListViewModel_OnItemRemoved;
            viewModel.DecisionListViewModel.ItemNameChanged -= DecisionListViewModel_OnItemNameChanged;
        }

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.DecisionListViewModel.ItemAdded += DecisionListViewModel_OnItemAdded;
            viewModel.DecisionListViewModel.ItemRemoved += DecisionListViewModel_OnItemRemoved;
            viewModel.DecisionListViewModel.ItemNameChanged += DecisionListViewModel_OnItemNameChanged;
        }

        private void DecisionListViewModel_OnItemAdded(DecisionItemViewModel item)
        {
            DecisionAdded?.Invoke(item);
        }

        private void DecisionListViewModel_OnItemRemoved(DecisionItemViewModel item)
        {
            TryRemoveItem(item.Name);
            DecisionRemoved?.Invoke(item);
        }

        private void DecisionListViewModel_OnItemNameChanged(DecisionItemViewModel item, string oldName, string newName)
        {
            Model.TryChangeDecisionName(oldName, newName);
            DecisionNameChanged?.Invoke(item, oldName, newName);
        }

        #endregion

        #region Items

        public override IReadOnlyList<DecisionModel> ItemModels => Model.Decisions;

        public override bool Contains(string name)
        {
            return Model.HasDecision(name);
        }

        protected override bool TryAddModelWithoutRecord(DecisionModel model, int index)
        {
            return Model.TryAddDecision(index, model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(DecisionModel model)
        {
            return Model.TryRemoveDecision(model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(string name, DecisionModel model)
        {
            return Model.TryRemoveDecision(name, model);
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveDecision(sourceIndex, destIndex);
        }

        #endregion
    }
}