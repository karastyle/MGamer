// <copyright file="InputView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputView : NameView<InputItemViewModel>
    {
        private ObjectEditorView<InputItemViewModel> editorView;
        private TextField categoryField;
        private Toggle hasNoTargetToggle;
        private Toggle enableCachePerTargetToggle;

        public InputView() : base(UIBuilderResourcePaths.NameView)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            CreateCategoryField();
            CreateHasNoTargetToggle();
            CreateEnableCachePerTargetToggle();

            CreateObjectEditorView();
        }

        private void CreateObjectEditorView()
        {
            editorView = new();
            // editorView.FieldValueChanged += _ => ViewModel.CalculateScore();
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

        private void CreateHasNoTargetToggle()
        {
            hasNoTargetToggle = new("Has No Target");
            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                ViewModel.HasNoTarget = hasNoTarget;
                enableCachePerTargetToggle.SetDisplay(!hasNoTarget);
            });
            Container.Add(hasNoTargetToggle);
        }

        private void CreateEnableCachePerTargetToggle()
        {
            enableCachePerTargetToggle = new("Enable Cache Per Target");
            enableCachePerTargetToggle.RegisterValueChangedCallback(evt =>
            {
                ViewModel.EnableCachePerTarget = evt.newValue;
            });
            Container.Add(enableCachePerTargetToggle);
        }

        protected override void OnUpdateView(InputItemViewModel viewModel)
        {
            editorView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(InputItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);
            this.TitleLabel.text = viewModel.TypeName;

            categoryField.SetDataBinding(
                nameof(TextField.value),
                nameof(InputItemViewModel.Category),
                BindingMode.ToTarget);

            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(InputItemViewModel.HasNoTarget),
                BindingMode.ToTarget);

            enableCachePerTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(InputItemViewModel.EnableCachePerTarget),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            categoryField.ClearBindings();
            hasNoTargetToggle.ClearBindings();
            enableCachePerTargetToggle.ClearBindings();
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