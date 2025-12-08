// <copyright file="DecisionSubViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionSubViewIntelligenceTab : UtilityIntelligenceViewMember
    {
        public DecisionSubViewIntelligenceTab()
        {
            TargetFilterView = new();
            TargetFilterView.Show(false);
            Add(TargetFilterView);

            ActionView = new();
            ActionView.SetEnabled(false);
            ActionView.Show(false);
            Add(ActionView);

            ConsiderationView = new();
            ConsiderationView.Show(false);
            Add(ConsiderationView);
        }

        public TargetFilterViewDecisionTab TargetFilterView { get; }
        public ObjectNameEditorView<ActionItemViewModelIntelligenceTab> ActionView { get; }

        public ConsiderationViewIntelligenceTab ConsiderationView { get; }

        public void ShowTargetFilterView(TargetFilterItemViewModelDecisionTab viewModel)
        {
            HideAllExcept(TargetFilterView);
            TargetFilterView.Show(true);

            TargetFilterView.UpdateView(viewModel);
        }

        public void HideTargetFilterView()
        {
            TargetFilterView.Show(false);
        }

        public void ShowActionEditorView(ActionItemViewModelIntelligenceTab viewModel)
        {
            HideAllExcept(ActionView);
            ActionView.Show(true);
            ActionView.UpdateView(viewModel);
        }

        public void HideActionEditorView()
        {
            ActionView.Show(false);
        }

        public void ShowConsiderationView(ConsiderationItemViewModelIntelligenceTab viewModel)
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
                HideTargetFilterView();

            if (ActionView != view)
                HideActionEditorView();

            if (ConsiderationView != view)
                HideConsiderationView();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            TargetFilterView.RootView = rootView;
            ActionView.RootView = rootView;
            ConsiderationView.RootView = rootView;
        }
    }
}