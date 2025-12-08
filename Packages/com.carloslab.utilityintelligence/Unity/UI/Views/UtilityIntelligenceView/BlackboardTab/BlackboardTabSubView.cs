// <copyright file="BlackboardTabSubView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class BlackboardTabSubView : UtilityIntelligenceViewMember
    {
        public BlackboardTabSubView()
        {
            VariableView = new();
            VariableView.Show(false);
            Add(VariableView);
        }
        public VariableView VariableView { get; }

        public void ShowVariableView(VariableViewModel viewModel)
        {
            VariableView.Show(true);
            VariableView.UpdateView(viewModel);
        }

        public void HideVariableView()
        {
            VariableView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            VariableView.RootView = rootView;
        }
    }
}