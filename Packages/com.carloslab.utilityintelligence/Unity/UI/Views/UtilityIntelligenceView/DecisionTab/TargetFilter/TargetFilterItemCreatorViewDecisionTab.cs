// <copyright file="TargetFilterItemCreatorViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemCreatorViewDecisionTab : ItemCreatorView<TargetFilterListViewModelDecisionTab, TargetFilterItemViewModelDecisionTab>
    {
        private static readonly TargetFilterItemViewModel CreateNewTargetFilter = new();
        private PopupField<TargetFilterItemViewModel> targetFilterField;

        protected override string CreateButtonText { get; } = "Add";

        public TargetFilterItemCreatorViewDecisionTab() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");

            targetFilterField = CreateTargetFilterField();
            typeFieldContainer.Add(targetFilterField);

            FormatTargetFilterField();
            HandleTargetFilterFieldValueChanged();
        }

        protected override void OnRefreshView(TargetFilterListViewModelDecisionTab viewModel)
        {
            UpdateTargetFilterFieldChoices(viewModel.TargetFilterListViewModel);
        }

        protected void ValidateCreateButtonByName()
        {
            TargetFilterItemViewModel targetFilter = targetFilterField.value;
            string name = targetFilter?.Name;
            bool isValidated = ValidateName(name);
            createButton.SetEnabled(isValidated);
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

        protected override void CreateNewItem()
        {
            TargetFilterItemViewModel targetFilter = targetFilterField.value;
            if (targetFilter != null) ViewModel.TryCreateItem(targetFilter.Model, out _);
        }

        #region Update TargetFilterField

        private void UpdateTargetFilterFieldChoices(TargetFilterListViewModel targetFiltersViewModel)
        {
            if (targetFiltersViewModel == null) return;

            targetFilterField.SetValueWithoutNotify(null);
            targetFilterField.choices.Clear();

            var targetFilters = targetFiltersViewModel.Items;
            for (int index = 0; index < targetFilters.Count; index++)
            {
                TargetFilterItemViewModel targetFilter = targetFilters[index];
                targetFilterField.choices.Add(targetFilter);
            }

            // if (targetFilterField.choices.Count > 0 && targetFilterField.value == null)
            //     targetFilterField.value = targetFilterField.choices[0];

            targetFilterField.choices.Sort(CompareChoices);

            int CompareChoices(TargetFilterItemViewModel choice1, TargetFilterItemViewModel choice2)
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

            targetFilterField.choices.Add(CreateNewTargetFilter);
        }

        #endregion

        #region Init TargetFilterField

        private PopupField<TargetFilterItemViewModel> CreateTargetFilterField()
        {
            PopupField<TargetFilterItemViewModel> targetFilterField = new();
            targetFilterField.label = "Name";
            return targetFilterField;
        }

        private void FormatTargetFilterField()
        {
            targetFilterField.formatListItemCallback = FormatListItem;
            targetFilterField.formatSelectedValueCallback = FormatSelectedItem;

            string FormatListItem(TargetFilterItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewTargetFilter)
                    return "CREATE NEW";

                string category = item.Category;
                if (!string.IsNullOrWhiteSpace(category))
                    return $"{category}/{item.Name}";

                return item.Name;
            }

            string FormatSelectedItem(TargetFilterItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewTargetFilter)
                    return "CREATE NEW";

                return item.Name;
            }
        }

        private void HandleTargetFilterFieldValueChanged()
        {
            targetFilterField.RegisterValueChangedCallback(evt =>
            {
                TargetFilterItemViewModel newTargetFilter = evt.newValue;

                if (newTargetFilter == CreateNewTargetFilter)
                {
                    RootView.SelectTargetFilterTab();
                    targetFilterField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateCreateButtonByName();
            });
        }

        #endregion

        #region ViewModel Events

        protected override void OnRegisterViewModelEvents(TargetFilterListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded += ViewModel_OnItemAdded;
            viewModel.ItemRemoved += ViewModel_OnItemRemoved;

            viewModel.TargetFilterAdded += ViewModel_OnTargetFilterAdded;
            viewModel.TargetFilterRemoved += ViewModel_OnTargetFilterRemoved;
            viewModel.TargetFilterNameChanged += ViewModel_OnTargetFilterNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(TargetFilterListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded -= ViewModel_OnItemAdded;
            viewModel.ItemRemoved -= ViewModel_OnItemRemoved;

            viewModel.TargetFilterAdded -= ViewModel_OnTargetFilterAdded;
            viewModel.TargetFilterRemoved -= ViewModel_OnTargetFilterRemoved;
            viewModel.TargetFilterNameChanged -= ViewModel_OnTargetFilterNameChanged;
        }

        private void ViewModel_OnTargetFilterNameChanged(TargetFilterItemViewModel item, string oldName, string newName)
        {
            if (targetFilterField.value == item)
                targetFilterField.SetValueWithoutNotify(item);
        }

        private void ViewModel_OnTargetFilterAdded(TargetFilterItemViewModel targetFilter)
        {
            var choices = targetFilterField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, targetFilter);
        }

        private void ViewModel_OnTargetFilterRemoved(TargetFilterItemViewModel targetFilter)
        {
            var targetFilters = targetFilterField.choices;
            if (targetFilters.Remove(targetFilter))
            {
                if (targetFilterField.value?.Name == targetFilter.Name)
                    targetFilterField.value = null;
            }
        }

        private void ViewModel_OnItemAdded(TargetFilterItemViewModelDecisionTab item)
        {
            ValidateCreateButtonByName();
        }

        private void ViewModel_OnItemRemoved(TargetFilterItemViewModelDecisionTab item)
        {
            ValidateCreateButtonByName();
        }

        #endregion
    }
}