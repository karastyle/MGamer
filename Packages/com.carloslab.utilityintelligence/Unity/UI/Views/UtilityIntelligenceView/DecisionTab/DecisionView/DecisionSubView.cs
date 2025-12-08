// <copyright file="DecisionSubView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionSubView : UtilityIntelligenceViewMember
    {
        public DecisionSubView()
        {
            TargetFilterView = new();
            TargetFilterView.Show(false);
            Add(TargetFilterView);

            ConsiderationView = new();
            ConsiderationView.Show(false);
            Add(ConsiderationView);

            ActionEditorView = new();
            ActionEditorView.Show(false);
            Add(ActionEditorView);
        }

        public TargetFilterViewDecisionTab TargetFilterView { get; }
        public ObjectNameEditorView<ActionItemViewModel> ActionEditorView { get; }

        public ConsiderationViewDecisionTab ConsiderationView { get; }

        public void ShowTargetFilterEditorView(TargetFilterItemViewModelDecisionTab viewModel)
        {
            HideAllExcept(TargetFilterView);
            if (IsRuntime) TargetFilterView.SetEnabled(false);
            TargetFilterView.Show(true);
            TargetFilterView.UpdateView(viewModel);
        }

        public void HideTargetFilterEditorView()
        {
            TargetFilterView.Show(false);
        }

        public void ShowActionEditorView(ActionItemViewModel viewModel)
        {
            HideAllExcept(ActionEditorView);
            // if(IsRuntime) ActionEditorView.SetEnabled(false);
            ActionEditorView.Show(true);
            ActionEditorView.UpdateView(viewModel);
        }

        public void HideActionEditorView()
        {
            ActionEditorView.Show(false);
        }

        public void ShowConsiderationView(ConsiderationItemViewModelDecisionTab viewModel)
        {
            HideAllExcept(ConsiderationView);
            ConsiderationView.Show(true);
            ConsiderationView.UpdateView(viewModel);
        }

        public void HideConsiderationView()
        {
            ConsiderationView.Show(false);
        }

        private void HideAllExcept(VisualElement view)
        {
            if (TargetFilterView != view)
                HideTargetFilterEditorView();

            if (ActionEditorView != view)
                HideActionEditorView();

            if (ConsiderationView != view)
                HideConsiderationView();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            TargetFilterView.RootView = rootView;
            ActionEditorView.RootView = rootView;
            ConsiderationView.RootView = rootView;
        }
    }
}