// <copyright file="InputViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputViewIntelligenceTab : UtilityIntelligenceViewMember<ConsiderationItemViewModelIntelligenceTab>
    {
        private readonly TextField inputField;

        private readonly InputValueViewIntelligenceTab inputValueView;

        private readonly TextField targetNameField;

        public InputViewIntelligenceTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input";
            Add(foldout);

            inputField = new("Name");
            inputField.SetEnabled(false);
            foldout.Add(inputField);

            inputValueView = new();
            foldout.Add(inputValueView);

            targetNameField = new("Target");
            targetNameField.SetEnabled(false);
            foldout.Add(targetNameField);
        }

        protected override void OnUpdateView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            inputValueView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputValueView.RootView = rootView;
        }

        protected override void OnRefreshView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            inputField.SetDataBinding(nameof(TextField.value), nameof(ConsiderationItemViewModelIntelligenceTab.InputName),
                BindingMode.ToTarget);

            targetNameField.dataSource = viewModel.ContextViewModel;
            targetNameField.SetDataBinding(nameof(TextField.value),
                nameof(ConsiderationContextViewModel.TargetName), BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            inputField.ClearBindings();
            targetNameField.ClearBindings();
        }
    }
}