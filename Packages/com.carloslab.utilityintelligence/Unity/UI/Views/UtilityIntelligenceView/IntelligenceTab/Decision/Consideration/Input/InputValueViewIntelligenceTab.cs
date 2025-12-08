// <copyright file="InputValueViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputValueViewIntelligenceTab : UtilityIntelligenceViewMember<ConsiderationItemViewModelIntelligenceTab>
    {
        private ValueField valueField;

        public InputValueViewIntelligenceTab() : base(null)
        {
        }

        protected override void OnRefreshView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            UpdateValueField(viewModel.Input);
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

            valueField.dataSource = ViewModel.ContextViewModel;
            valueField.SetValueDataBinding(nameof(ConsiderationContextViewModel.RawInput), BindingMode.TwoWay);

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

        private void EnableValueField()
        {
            if (valueField != null)
                valueField.SetEnabled(true);
        }

        private void DisableValueField()
        {
            if (valueField != null)
                valueField.SetEnabled(false);
        }

        protected override void OnRegisterViewModelEvents(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            viewModel.InputChanged += ViewModel_OnInputChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            viewModel.InputChanged -= ViewModel_OnInputChanged;
        }

        private void ViewModel_OnInputChanged(InputItemViewModel newInput)
        {
            UpdateValueField(newInput);
            UpdateRuntimeMode();
        }
    }
}