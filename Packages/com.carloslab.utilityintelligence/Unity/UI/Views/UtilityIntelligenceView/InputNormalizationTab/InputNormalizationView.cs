// <copyright file="InputNormalizationView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationView : NameView<InputNormalizationItemViewModel>
    {
        private InputViewNormalizationTab inputView;

        private ObjectEditorView<InputNormalizationItemViewModel> editorView;
        private TextField categoryField;
        private Toggle hasNoTargetToggle;
        private Toggle enableCachePerTargetToggle;
        private FloatField normalizedInputField;

        public InputNormalizationView() : base(UIBuilderResourcePaths.NameView)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            CreateCategoryField();

            CreateHasNoTargetToggle();

            CreateEnableCachePerTargetToggle();

            CreateEditorView();

            CreateNormalizedInputField();

            inputView = new();
            Container.Add(inputView);
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

        private void CreateEditorView()
        {
            editorView = new();
            editorView.FieldValueChanged += _ => ViewModel.EvaluateInputNormalization();
            Container.Add(editorView);
        }

        private void CreateNormalizedInputField()
        {
            normalizedInputField = new FloatField("Normalized Input");
            normalizedInputField.SetEnabled(false);
            Container.Add(normalizedInputField);
        }

        protected override void OnUpdateView(InputNormalizationItemViewModel viewModel)
        {
            editorView.UpdateView(viewModel);
            inputView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(InputNormalizationItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);
            this.TitleLabel.text = viewModel.TypeName;

            categoryField.SetDataBinding(
                nameof(TextField.value),
                nameof(InputNormalizationItemViewModel.Category),
                BindingMode.ToTarget);

            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(InputNormalizationItemViewModel.HasNoTarget),
                BindingMode.ToTarget);

            enableCachePerTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(InputNormalizationItemViewModel.EnableCachePerTarget),
                BindingMode.ToTarget);
        }

        protected override void OnEnableEditMode()
        {
            normalizedInputField.SetDataBinding(nameof(FloatField.value),
                nameof(InputNormalizationItemViewModel.NormalizedInput), BindingMode.ToTarget);
        }

        protected override void OnEnableRuntimeMode()
        {
            categoryField.SetEnabled(false);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            categoryField.ClearBindings();
            hasNoTargetToggle.ClearBindings();
            normalizedInputField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputView.RootView = rootView;
            editorView.RootView = rootView;
        }
    }
}