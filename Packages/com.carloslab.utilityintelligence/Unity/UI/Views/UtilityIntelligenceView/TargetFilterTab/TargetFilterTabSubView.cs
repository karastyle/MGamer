// <copyright file="TargetFilterTabSubView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterTabSubView : UtilityIntelligenceViewMember
    {
        public TargetFilterView TargetFilterView { get; }
        public TargetFilterTabSubView()
        {
            TargetFilterView = new();
            TargetFilterView.Show(false);
            Add(TargetFilterView);
        }


        public void ShowEditorView(TargetFilterItemViewModel viewModel)
        {
            TargetFilterView.Show(true);
            TargetFilterView.UpdateView(viewModel);
        }

        public void HideEditorView()
        {
            TargetFilterView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            TargetFilterView.RootView = rootView;
        }
    }
}