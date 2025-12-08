// <copyright file="ConsiderationItemCreatorViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationItemCreatorViewDecisionTab
        : ItemCreatorView<ConsiderationListViewModelDecisionTab, ConsiderationItemViewModelDecisionTab>
    {
        private static readonly ConsiderationItemViewModel CreateNewConsideration = new();
        private PopupField<ConsiderationItemViewModel> considerationField;


        protected override string CreateButtonText { get; } = "Add";

        public ConsiderationItemCreatorViewDecisionTab() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {

        }

        #region View Functions

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            considerationField = CreateConsiderationField();
            typeFieldContainer.Add(considerationField);

            FormatConsiderationField();
            HandleConsiderationFieldValueChanged();
        }

        protected override void OnRefreshView(ConsiderationListViewModelDecisionTab viewModel)
        {
            UpdateConsiderationFieldChoices(viewModel.ConsiderationListViewModel);
        }

        protected override void CreateNewItem()
        {
            ConsiderationItemViewModel consideration = considerationField.value;
            if (consideration != null) ViewModel.TryCreateItem(consideration.Model, out _);
        }

        #endregion

        #region Helper Functions

        private void ValidateCreateButtonByName()
        {
            ConsiderationItemViewModel consideration = considerationField.value;
            string name = consideration?.Name;
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

        #region Init ConsiderationField

        private PopupField<ConsiderationItemViewModel> CreateConsiderationField()
        {
            PopupField<ConsiderationItemViewModel> considerationField = new();
            considerationField.label = "Name";
            return considerationField;
        }

        private void FormatConsiderationField()
        {
            considerationField.formatListItemCallback = FormatListItem;
            considerationField.formatSelectedValueCallback = FormatSelectedItem;

            string FormatListItem(ConsiderationItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewConsideration)
                    return "CREATE NEW";

                string category = item.Category;
                if (!string.IsNullOrWhiteSpace(category))
                    return $"{category}/{item.Name}";

                var input = item.InputViewModel;
                if (input != null)
                    return $"{input.Name}/{item.Name}";

                return item.Name;
            }

            string FormatSelectedItem(ConsiderationItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewConsideration)
                    return "CREATE NEW";

                return item.Name;
            }
        }

        private void HandleConsiderationFieldValueChanged()
        {
            considerationField.RegisterValueChangedCallback(evt =>
            {
                ConsiderationItemViewModel newConsideration = evt.newValue;

                if (newConsideration == CreateNewConsideration)
                {
                    RootView.SelectConsiderationTab();
                    considerationField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateCreateButtonByName();
            });
        }

        #endregion

        #region Update ConsiderationField

        private void UpdateConsiderationFieldChoices(ConsiderationListViewModel editorListViewModel)
        {
            if (editorListViewModel == null) return;

            considerationField.SetValueWithoutNotify(null);
            considerationField.choices.Clear();

            var considerations = editorListViewModel.Items;
            for (int index = 0; index < considerations.Count; index++)
            {
                ConsiderationItemViewModel consideration = considerations[index];
                considerationField.choices.Add(consideration);
            }

            // if (considerationField.choices.Count > 0 && considerationField.value == null)
            //     considerationField.value = considerationField.choices[0];

            considerationField.choices.Sort(CompareChoices);

            int CompareChoices(ConsiderationItemViewModel choice1, ConsiderationItemViewModel choice2)
            {
                if (choice1 == null)
                    return -1;
                if (choice2 == null)
                    return 1;

                int result;

                var category1 = choice1.Category;
                var category2 = choice2.Category;
                if (string.IsNullOrWhiteSpace(category1) && string.IsNullOrWhiteSpace(category2))
                {
                    string inputName1 = choice1.InputViewModel?.Name;
                    string inputName2 = choice2.InputViewModel?.Name;

                    if (string.IsNullOrWhiteSpace(inputName1) && string.IsNullOrWhiteSpace(inputName2))
                        return string.CompareOrdinal(choice1.Name, choice2.Name);

                    if (string.IsNullOrWhiteSpace(inputName1))
                        return 1;

                    if (string.IsNullOrWhiteSpace(inputName2))
                        return -1;

                    result = string.CompareOrdinal(inputName1, inputName2);
                    if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);

                    return result;
                }

                if (string.IsNullOrWhiteSpace(category1))
                    return 1;

                if (string.IsNullOrWhiteSpace(category2))
                    return -1;

                result = string.CompareOrdinal(category1, category2);
                if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
                return result;
            }

            considerationField.choices.Add(CreateNewConsideration);
        }

        #endregion

        #region ViewModel Events

        protected override void OnRegisterViewModelEvents(ConsiderationListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded += ViewModel_OnItemAdded;
            viewModel.ItemRemoved += ViewModel_OnItemRemoved;

            viewModel.ConsiderationAdded += ViewModel_OnConsiderationAdded;
            viewModel.ConsiderationRemoved += ViewModel_OnConsiderationRemoved;
            viewModel.ConsiderationNameChanged += ViewModel_OnConsiderationNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded -= ViewModel_OnItemAdded;
            viewModel.ItemRemoved -= ViewModel_OnItemRemoved;

            viewModel.ConsiderationAdded -= ViewModel_OnConsiderationAdded;
            viewModel.ConsiderationRemoved -= ViewModel_OnConsiderationRemoved;
            viewModel.ConsiderationNameChanged -= ViewModel_OnConsiderationNameChanged;
        }

        private void ViewModel_OnConsiderationNameChanged(ConsiderationItemViewModel item, string oldName, string newName)
        {
            if (considerationField.value == item)
                considerationField.SetValueWithoutNotify(item);
        }

        private void ViewModel_OnConsiderationAdded(ConsiderationItemViewModel consideration)
        {
            var choices = considerationField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, consideration);
        }

        private void ViewModel_OnConsiderationRemoved(ConsiderationItemViewModel consideration)
        {
            var considerations = considerationField.choices;
            if (considerations.Remove(consideration))
            {
                if (considerationField.value?.Name == consideration.Name)
                    considerationField.value = null;
            }
        }

        private void ViewModel_OnItemAdded(ConsiderationItemViewModelDecisionTab item)
        {
            ValidateCreateButtonByName();
            MakeDecision();
        }

        private void ViewModel_OnItemRemoved(ConsiderationItemViewModelDecisionTab item)
        {
            ValidateCreateButtonByName();

            MakeDecision();
        }

        #endregion
    }
}