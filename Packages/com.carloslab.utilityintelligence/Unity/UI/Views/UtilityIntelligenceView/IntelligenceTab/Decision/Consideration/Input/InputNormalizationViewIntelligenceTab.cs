// <copyright file="InputNormalizationViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationViewIntelligenceTab : UtilityIntelligenceViewMember<ConsiderationItemViewModelIntelligenceTab>
    {
        private readonly TextField nameField;
        private readonly FloatField normalizedInputField;

        private readonly TextField targetNameField;

        private InputViewIntelligenceTab inputView;

        public InputNormalizationViewIntelligenceTab() : base(null)
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

            targetNameField = new TextField("Target");
            targetNameField.SetEnabled(false);
            foldout.Add(targetNameField);

            inputView = new();
            inputView.style.marginTop = 5;
            Add(inputView);
        }

        protected override void OnUpdateView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            inputView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputView.RootView = rootView;
        }

        protected override void OnRefreshView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            nameField.SetDataBinding(nameof(TextField.value), nameof(ConsiderationItemViewModelIntelligenceTab.InputNormalizationName),
                BindingMode.ToTarget);

            normalizedInputField.dataSource = viewModel.ContextViewModel;
            normalizedInputField.SetDataBinding(nameof(FloatField.value),
                nameof(ConsiderationContextViewModel.NormalizedInput), BindingMode.ToTarget);

            targetNameField.dataSource = viewModel.ContextViewModel;
            targetNameField.SetDataBinding(nameof(TextField.value),
                nameof(ConsiderationContextViewModel.TargetName), BindingMode.ToTarget);

            UpdateNormalizationInputDisplay(viewModel.InputNormalizationViewModel);
        }

        protected override void OnResetView()
        {
            nameField.ClearBindings();
            normalizedInputField.ClearBindings();
            targetNameField.ClearBindings();
        }

        protected override void OnRegisterViewModelEvents(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            viewModel.InputNormalizationChanged += OnInputNormalizationChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            viewModel.InputNormalizationChanged -= OnInputNormalizationChanged;
        }

        private void OnInputNormalizationChanged(InputNormalizationItemViewModel viewModel)
        {
            UpdateNormalizationInputDisplay(viewModel);
        }

        private void UpdateNormalizationInputDisplay(InputNormalizationItemViewModel viewMode)
        {
            if (viewMode != null)
                normalizedInputField.SetDisplay(true);
            else
                normalizedInputField.SetDisplay(false);
        }
    }
}