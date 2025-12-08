// <copyright file="InputViewNormalizationTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputViewNormalizationTab : UtilityIntelligenceViewMember<InputNormalizationItemViewModel>
    {

        public InputViewNormalizationTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input";
            Add(foldout);

            CreateInputField(foldout);

            CreateInputValueView(foldout);
        }

        #region InputView

        protected override void OnUpdateView(InputNormalizationItemViewModel viewModel)
        {
            if (IsRuntime)
                inputField.SetEnabled(false);

            inputValueView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(InputNormalizationItemViewModel viewModel)
        {
            UpdateInputFieldChoices(viewModel.InputListViewModel, viewModel.InputValueType);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputValueView.RootView = rootView;
        }

        protected override void OnModelChanged(IModel newModel)
        {
            if (newModel == null) return;

            UpdateInputFieldChoices(ViewModel.InputListViewModel, ViewModel.InputValueType);
        }

        #endregion

        #region ViewModel Events

        protected override void OnRegisterViewModelEvents(InputNormalizationItemViewModel viewModel)
        {
            viewModel.InputAdded += ViewModel_OnInputAdded;
            viewModel.InputRemoved += ViewModel_OnInputRemoved;
            viewModel.InputNameChanged += ViewModel_OnInputNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(InputNormalizationItemViewModel viewModel)
        {
            viewModel.InputAdded -= ViewModel_OnInputAdded;
            viewModel.InputRemoved -= ViewModel_OnInputRemoved;
            viewModel.InputNameChanged -= ViewModel_OnInputNameChanged;
        }

        private void ViewModel_OnInputNameChanged(InputItemViewModel inputViewModel, string oldName, string newName)
        {
            if (inputField.value == inputViewModel)
                inputField.SetValueWithoutNotify(inputViewModel);
        }

        private void ViewModel_OnInputAdded(InputItemViewModel inputViewModel)
        {
            var choices = inputField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, inputViewModel);
        }

        private void ViewModel_OnInputRemoved(InputItemViewModel inputViewModel)
        {
            if (inputField.value == inputViewModel)
                inputField.value = null;

            inputField.choices.Remove(inputViewModel);
        }

        #endregion

        #region InputValueView

        private InputValueViewNormalizationTab inputValueView;

        private void CreateInputValueView(Foldout foldout)
        {
            inputValueView = new();
            foldout.Add(inputValueView);
        }

        #endregion

        #region Create InputField

        private static readonly InputItemViewModel CreateNewInput = new();

        private PopupField<InputItemViewModel> inputField;

        private void CreateInputField(Foldout foldout)
        {
            inputField = new();
            inputField.label = "Name";
            FormatInputField();
            HandleInputFieldValueChanged();
            foldout.Add(inputField);
        }

        private void FormatInputField()
        {
            inputField.formatListItemCallback = FormatListItem;

            inputField.formatSelectedValueCallback = FormatSelectedItem;

            string FormatListItem(InputItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewInput)
                    return "CREATE NEW";

                string category = item.Category;
                if (!string.IsNullOrWhiteSpace(category))
                    return $"{category}/{item.Name}";

                return item.Name;
            }

            string FormatSelectedItem(InputItemViewModel item)
            {
                if (item == null)
                    return "None";

                if (item == CreateNewInput)
                    return "CREATE NEW";

                return item.Name;
            }
        }

        private void HandleInputFieldValueChanged()
        {
            inputField.RegisterValueChangedCallback(evt =>
            {
                InputItemViewModel newInput = evt.newValue;

                if (newInput == CreateNewInput)
                {
                    RootView.SelectInputTab();

                    inputField.SetValueWithoutNotify(evt.previousValue);
                }
                else if (evt.previousValue != CreateNewInput)
                {
                    ViewModel.InputName = newInput?.Name;
                }
            });
        }

        #endregion

        #region Update InputField

        private void UpdateInputFieldChoices(InputListViewModel inputsViewModel, Type inputValueType)
        {
            if (inputsViewModel == null) return;

            ResetInputFieldWithoutNotify();

            inputField.choices.Add(null);

            var inputs = inputsViewModel.Items;
            for (int index = 0; index < inputs.Count; index++)
            {
                InputItemViewModel input = inputs[index];

                if (input.ValueType == inputValueType)
                    inputField.choices.Add(input);

                if (input.Name == ViewModel.Model.InputName)
                    inputField.value = input;
            }

            inputField.choices.Sort(CompareChoices);

            int CompareChoices(InputItemViewModel choice1, InputItemViewModel choice2)
            {
                if (choice1 == null)
                    return -1;

                if (choice2 == null)
                    return 1;

                var category1 = choice1.Category;
                var category2 = choice2.Category;

                if (string.IsNullOrWhiteSpace(category1) && string.IsNullOrWhiteSpace(category2))
                    return string.CompareOrdinal(choice1.Name, choice2.Name);

                if (string.IsNullOrWhiteSpace(category1))
                    return 1;

                if (string.IsNullOrWhiteSpace(category2))
                    return -1;

                int result = string.CompareOrdinal(category1, category2);
                if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
                return result;
            }

            inputField.choices.Add(CreateNewInput);
        }

        private void ResetInputFieldWithoutNotify()
        {
            inputField.SetValueWithoutNotify(null);
            inputField.choices.Clear();
        }

        #endregion
    }
}