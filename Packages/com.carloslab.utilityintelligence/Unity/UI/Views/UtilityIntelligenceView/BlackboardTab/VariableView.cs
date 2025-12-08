// <copyright file="VariableView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableView : NameView<VariableViewModel>
    {
        private TextField categoryField;
        private ObjectEditorView<VariableViewModel> editorView;

        public VariableView() : base(UIBuilderResourcePaths.NameView)
        {
            CreateCategoryField();
            CreateObjectEditorView();
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

        private void CreateObjectEditorView()
        {
            editorView = new();
            // editorView.FieldValueChanged += _ => ViewModel.CalculateScore();
            Container.Add(editorView);
        }

        protected override void OnUpdateView(VariableViewModel viewModel)
        {
            editorView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(VariableViewModel viewModel)
        {
            base.OnRefreshView(viewModel);

            this.TitleLabel.text = viewModel.TypeName;
            categoryField.SetValueWithoutNotify(viewModel.Category);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            categoryField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            editorView.RootView = rootView;
        }

        protected override void OnEnableRuntimeMode()
        {
            categoryField.SetEnabled(false);
        }
    }
}