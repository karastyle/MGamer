// <copyright file="DecisionItemCreatorViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionItemCreatorViewIntelligenceTab : ItemCreatorView<DecisionListViewModelIntelligenceTab, DecisionItemViewModelIntelligenceTab>
    {
        private static readonly DecisionItemViewModel CreateNewDecision = new();
        private PopupField<DecisionItemViewModel> decisionField;

        protected override string CreateButtonText { get; } = "Add";

        public DecisionItemCreatorViewIntelligenceTab() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {
        }

        #region View Functions

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            decisionField = CreateDecisionField();
            typeFieldContainer.Add(decisionField);

            FormatDecisionField();
            HandleDecisionFieldValueChanged();
        }

        protected override void OnRefreshView(DecisionListViewModelIntelligenceTab viewModel)
        {
            UpdateDecisionFieldChoices(viewModel.DecisionListViewModel);
        }

        protected override void CreateNewItem()
        {
            var decision = decisionField.value;
            if (decision != null) ViewModel.TryCreateItem(decision.Model, out _);
        }

        #endregion

        #region Init DecisionField

        private PopupField<DecisionItemViewModel> CreateDecisionField()
        {
            PopupField<DecisionItemViewModel> decisionField = new();
            decisionField.label = "Name";
            return decisionField;
        }

        private void FormatDecisionField()
        {
            decisionField.formatListItemCallback = FormatListItem;
            decisionField.formatSelectedValueCallback = FormatSelectedItem;

            string FormatListItem(DecisionItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewDecision)
                    return "CREATE NEW";

                string category = item.Category;
                if (!string.IsNullOrWhiteSpace(category))
                    return $"{category}/{item.Name}";

                return item.Name;
            }

            string FormatSelectedItem(DecisionItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewDecision)
                    return "CREATE NEW";

                return item.Name;
            }
        }

        private void HandleDecisionFieldValueChanged()
        {
            decisionField.RegisterValueChangedCallback(evt =>
            {
                var newDecision = evt.newValue;

                if (newDecision == CreateNewDecision)
                {
                    RootView.SelectDecisionTab();

                    decisionField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateCreateButtonByName();
            });
        }

        #endregion

        #region Update DecisionField

        private void UpdateDecisionFieldChoices(DecisionListViewModel editorListViewModel)
        {
            if (editorListViewModel == null) return;

            decisionField.SetValueWithoutNotify(null);
            decisionField.choices.Clear();

            var decisions = editorListViewModel.Items;
            for (int index = 0; index < decisions.Count; index++)
            {
                var decision = decisions[index];
                decisionField.choices.Add(decision);
            }
            //
            // if (decisionField.choices.Count > 0 && decisionField.value == null)
            //     decisionField.value = decisionField.choices[0];

            decisionField.choices.Sort(CompareChoices);

            int CompareChoices(DecisionItemViewModel choice1, DecisionItemViewModel choice2)
            {
                if (choice1 == null)
                    return -1;
                if (choice2 == null)
                    return 1;

                int result;

                var category1 = choice1.Category;
                var category2 = choice2.Category;
                if (string.IsNullOrWhiteSpace(category1) && string.IsNullOrWhiteSpace(category2))
                    return string.CompareOrdinal(choice1.Name, choice2.Name);

                if (string.IsNullOrWhiteSpace(category1))
                    return 1;

                if (string.IsNullOrWhiteSpace(category2))
                    return -1;

                result = string.CompareOrdinal(category1, category2);
                if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
                return result;
            }

            decisionField.choices.Add(CreateNewDecision);
        }

        #endregion

        #region Helper Functions

        private void ValidateCreateButtonByName()
        {
            var decision = decisionField.value;
            string name = decision?.Name;
            bool isValid = ValidateName(name);
            createButton.SetEnabled(isValid);
        }

        private bool ValidateName(string name)
        {
            if (IsRuntime) return false;

            if (string.IsNullOrEmpty(name))
                return false;

            if (ViewModel == null || ViewModel.Contains(name))
                return false;

            return true;
        }

        public void MakeDecision()
        {
            var intelligenceViewModel = ViewModel.RootViewModel;
            intelligenceViewModel.MakeDecision();
        }

        #endregion

        #region ViewModel Events

        protected override void OnRegisterViewModelEvents(DecisionListViewModelIntelligenceTab viewModel)
        {
            viewModel.ItemAdded += ViewModel_OnItemAdded;
            viewModel.ItemRemoved += ViewModel_OnItemRemoved;

            viewModel.DecisionAdded += ViewModel_OnDecisionAdded;
            viewModel.DecisionRemoved += ViewModel_OnDecisionRemoved;
            viewModel.DecisionNameChanged += ViewModel_OnDecisionNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(DecisionListViewModelIntelligenceTab viewModel)
        {
            viewModel.ItemAdded -= ViewModel_OnItemAdded;
            viewModel.ItemRemoved -= ViewModel_OnItemRemoved;

            viewModel.DecisionAdded -= ViewModel_OnDecisionAdded;
            viewModel.DecisionRemoved -= ViewModel_OnDecisionRemoved;
            viewModel.DecisionNameChanged -= ViewModel_OnDecisionNameChanged;
        }

        private void ViewModel_OnDecisionNameChanged(DecisionItemViewModel item, string oldName, string newName)
        {
            if (decisionField.value == item)
                decisionField.SetValueWithoutNotify(item);
        }

        private void ViewModel_OnDecisionAdded(DecisionItemViewModel decision)
        {
            var choices = decisionField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, decision);
        }

        private void ViewModel_OnDecisionRemoved(DecisionItemViewModel decision)
        {
            var decisions = decisionField.choices;
            if (decisions.Remove(decision))
            {
                if (decisionField.value?.Name == decision.Name)
                    decisionField.value = null;
            }
        }

        private void ViewModel_OnItemAdded(DecisionItemViewModelIntelligenceTab item)
        {
            ValidateCreateButtonByName();
            MakeDecision();
        }

        private void ViewModel_OnItemRemoved(DecisionItemViewModelIntelligenceTab item)
        {
            ValidateCreateButtonByName();

            MakeDecision();
        }

        #endregion
    }
}