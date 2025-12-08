// <copyright file="TargetFilterViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterViewDecisionTab : NameView<TargetFilterItemViewModelDecisionTab>
    {
        private ObjectEditorView<TargetFilterItemViewModelDecisionTab> editorView;

        private Button editButton;

        public TargetFilterViewDecisionTab() : base(UIBuilderResourcePaths.NameView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            TitleLabel.text = "Target Filter";

            editorView = new();
            editorView.SetEnabled(false);
            Container.Add(editorView);

            editButton = new Button
            {
                text = "Edit",
                style =
                {
                    width = 100,
                    marginTop = 20,
                    alignSelf = Align.Center
                }
            };

            editButton.clicked += SwitchToEditView;
            Container.Add(editButton);
        }

        protected override void OnUpdateView(TargetFilterItemViewModelDecisionTab viewModel)
        {
            editorView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            editorView.RootView = rootView;
        }

        private void SwitchToEditView()
        {
            RootView.SelectTargetFilterTab(ViewModel.TargetFilterViewModel.Index);
        }
    }
}