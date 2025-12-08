// <copyright file="InputViewConsiderationTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class InputViewConsiderationTab : UtilityIntelligenceViewMember<ConsiderationItemViewModel>
    {
        private readonly TextField inputField;
        private readonly InputValueViewConsiderationTab inputValueView;

        public InputViewConsiderationTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input";
            Add(foldout);

            inputField = new("Name");
            inputField.SetEnabled(false);
            foldout.Add(inputField);

            inputValueView = new();
            foldout.Add(inputValueView);

        }

        protected override void OnUpdateView(ConsiderationItemViewModel viewModel)
        {
            inputValueView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(ConsiderationItemViewModel viewModel)
        {
            inputField.SetDataBinding(nameof(TextField.value), nameof(ConsiderationItemViewModel.InputName),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            inputField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputValueView.RootView = rootView;
        }
    }
}