// <copyright file="ConsiderationView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationView : NameView<ConsiderationItemViewModel>
    {
        private TextField categoryField;
        private Toggle hasNoTargetToggle;
        private Toggle enableCachePerTargetToggle;

        private InputNormalizationViewConsiderationTab inputNormalizationView;
        private ResponseCurveView responseCurveView;

        public ConsiderationView() : base(UIBuilderResourcePaths.ConsiderationView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            InitCategoryField();

            InitHasNoTargetToggle();

            InitEnableCachePerTargetToggle();

            // AddZoomButtons();

            inputNormalizationView = this.Q<InputNormalizationViewConsiderationTab>();

            responseCurveView = this.Q<ResponseCurveView>();
        }

        private void AddZoomButtons()
        {
            var resetButton = new Button { text = "Reset Zoom" };
            resetButton.RegisterCallback<ClickEvent>(ev => this.transform.scale = Vector3.one);
            var zoomButton = new Button { text = "Zoom x 2" };
            zoomButton.RegisterCallback<ClickEvent>(ev => this.transform.scale *= 3.5f);
            Add(resetButton);
            Add(zoomButton);
        }

        private void InitCategoryField()
        {
            categoryField = this.Q<TextField>("CategoryField");

            categoryField.isDelayed = true;
            categoryField.RegisterValueChangedCallback(evt =>
            {
                ViewModel.Category = evt.newValue;
            });
        }

        private void InitHasNoTargetToggle()
        {
            hasNoTargetToggle = this.Q<Toggle>("HasNoTargetToggle");

            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                ViewModel.HasNoTarget = hasNoTarget;
                enableCachePerTargetToggle.SetDisplay(!hasNoTarget);
            });
        }

        private void InitEnableCachePerTargetToggle()
        {
            enableCachePerTargetToggle = this.Q<Toggle>("EnableCachePerTargetToggle");
            enableCachePerTargetToggle.RegisterValueChangedCallback(evt =>
            {
                ViewModel.EnableCachePerTarget = evt.newValue;
            });
        }

        protected override void OnUpdateView(ConsiderationItemViewModel viewModel)
        {
            inputNormalizationView.UpdateView(viewModel);
            responseCurveView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(ConsiderationItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);

            categoryField.SetDataBinding(
                nameof(TextField.value),
                nameof(ConsiderationItemViewModel.Category),
                BindingMode.ToTarget);

            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(ConsiderationItemViewModel.HasNoTarget),
                BindingMode.ToTarget);

            enableCachePerTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(ConsiderationItemViewModel.EnableCachePerTarget),
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
            inputNormalizationView.RootView = rootView;
            responseCurveView.RootView = rootView;
        }

        protected override void OnEnableRuntimeMode()
        {
            categoryField.SetEnabled(false);
        }
    }
}