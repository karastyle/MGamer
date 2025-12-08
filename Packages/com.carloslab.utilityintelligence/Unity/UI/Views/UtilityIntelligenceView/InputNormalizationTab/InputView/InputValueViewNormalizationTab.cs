// <copyright file="InputValueViewNormalizationTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputValueViewNormalizationTab : UtilityIntelligenceViewMember<InputNormalizationItemViewModel>
    {
        private ValueField valueField;

        public InputValueViewNormalizationTab() : base(null)
        {
        }

        protected override void OnRefreshView(InputNormalizationItemViewModel viewModel)
        {
            UpdateValueField(viewModel.InputViewModel);
        }

        protected override void OnResetView()
        {
            RemoveValueField();
        }

        private void UpdateValueField(InputItemViewModel input)
        {
            RemoveValueField();
            CreateValueField(input);
        }

        private void RemoveValueField()
        {
            if (valueField != null)
            {
                Remove(valueField);
                valueField = null;
            }
        }

        private void CreateValueField(InputItemViewModel input)
        {
            if (input == null) return;

            var valueField = new ValueField(input.ValueType, false, "Value");
            if (valueField.IsValid == false) return;

            valueField.SetEnabled(false);

            Add(valueField);

            this.valueField = valueField;
        }

        protected override void OnEnableEditMode()
        {
            EnableValueField();
        }

        protected override void OnEnableRuntimeMode()
        {
            DisableValueField();
        }

        private void DisableValueField()
        {
            if (valueField == null) return;

            valueField.ClearBindings();
            valueField.SetEnabled(false);
        }

        private void EnableValueField()
        {
            if (valueField == null) return;

            valueField.SetValueDataBinding(nameof(ConsiderationItemViewModel.RawInput), BindingMode.TwoWay);
            valueField.SetEnabled(true);
        }

        protected override void OnRegisterViewModelEvents(InputNormalizationItemViewModel viewModel)
        {
            viewModel.InputChanged += ViewModel_OnInputChanged;
        }

        protected override void OnUnregisterViewModelEvents(InputNormalizationItemViewModel viewModel)
        {
            viewModel.InputChanged -= ViewModel_OnInputChanged;
        }

        private void ViewModel_OnInputChanged(InputItemViewModel newInput)
        {
            UpdateValueField(newInput);
            if (IsRuntime)
                DisableValueField();
            else
                EnableValueField();
        }
    }
}