// <copyright file="InputTabSubView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputTabSubView : UtilityIntelligenceViewMember
    {
        public InputTabSubView()
        {
            InputView = new();
            InputView.Show(false);
            Add(InputView);
        }

        public InputView InputView { get; }

        public void ShowInputEditorView(InputItemViewModel viewModel)
        {
            InputView.Show(true);
            InputView.UpdateView(viewModel);
        }

        public void HideInputEditorView()
        {
            InputView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            InputView.RootView = rootView;
        }
    }
}