// <copyright file="TargetFilterView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterView : NameView<TargetFilterItemViewModel>
    {
        private TextField categoryField;
        private ObjectEditorView<TargetFilterItemViewModel> editorView;

        public TargetFilterView() : base(UIBuilderResourcePaths.NameView)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            CreateCategoryField();

            CrateObjectEditorView();
        }

        private void CrateObjectEditorView()
        {
            editorView = new();
            Container.Add(editorView);
        }

        private void CreateCategoryField()
        {
            categoryField = new("Category");
            categoryField.isDelayed = true;
            categoryField.RegisterValueChangedCallback(evt =>
            {
                ViewModel.Category = evt.newValue;
            });
            Container.Add(categoryField);
        }

        protected override void OnUpdateView(TargetFilterItemViewModel viewModel)
        {
            editorView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(TargetFilterItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);
            TitleLabel.text = viewModel.TypeName;

            categoryField.SetDataBinding(
                nameof(TextField.value),
                nameof(TargetFilterItemViewModel.Category),
                BindingMode.ToTarget);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            base.OnResetView();
            editorView.RootView = rootView;
        }

        protected override void OnEnableRuntimeMode()
        {
            categoryField.SetEnabled(false);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            categoryField.ClearBindings();
        }
    }
}