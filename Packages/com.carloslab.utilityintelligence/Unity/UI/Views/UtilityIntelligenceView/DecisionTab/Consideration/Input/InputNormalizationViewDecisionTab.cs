// <copyright file="InputNormalizationViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationViewDecisionTab : UtilityIntelligenceViewMember<ConsiderationItemViewModelDecisionTab>
    {
        private readonly TextField nameField;
        private readonly FloatField normalizedInputField;
        private InputViewConsiderationTab inputView;

        public InputNormalizationViewDecisionTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input Normalization";
            Add(foldout);

            nameField = new TextField("Type");
            nameField.SetEnabled(false);
            foldout.Add(nameField);

            normalizedInputField = new FloatField("Normalized Input");
            normalizedInputField.SetEnabled(false);
            foldout.Add(normalizedInputField);

            inputView = new();
            inputView.style.marginTop = 5;
            Add(inputView);
        }

        protected override void OnUpdateView(ConsiderationItemViewModelDecisionTab viewModel)
        {
            inputView.UpdateView(viewModel?.ConsiderationViewModel);
        }

        protected override void OnRefreshView(ConsiderationItemViewModelDecisionTab viewModel)
        {
            nameField.SetDataBinding(nameof(TextField.value), nameof(ConsiderationItemViewModelDecisionTab.InputNormalizationName),
                BindingMode.ToTarget);

            normalizedInputField.dataSource = viewModel.ConsiderationViewModel;
            UpdateNormalizationInputDisplay(viewModel.InputNormalization);
        }

        protected override void OnResetView()
        {
            nameField.ClearBindings();
            normalizedInputField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputView.RootView = rootView;
        }

        protected override void OnRegisterViewModelEvents(ConsiderationItemViewModelDecisionTab viewModel)
        {
            viewModel.InputNormalizationChanged += ViewModel_OnInputNormalizationChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationItemViewModelDecisionTab viewModel)
        {
            viewModel.InputNormalizationChanged -= ViewModel_OnInputNormalizationChanged;
        }

        private void ViewModel_OnInputNormalizationChanged(InputNormalizationItemViewModel viewModel)
        {
            UpdateNormalizationInputDisplay(viewModel);
        }

        protected override void OnEnableEditMode()
        {
            normalizedInputField.SetDataBinding(nameof(FloatField.value),
                nameof(ConsiderationItemViewModel.NormalizedInput), BindingMode.ToTarget);
        }

        private void UpdateNormalizationInputDisplay(InputNormalizationItemViewModel viewModel)
        {
            if (viewModel != null)
                normalizedInputField.SetDisplay(true);
            else
                normalizedInputField.SetDisplay(false);
        }
    }
}