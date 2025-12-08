// <copyright file="DecisionMakerMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerMainView : NameMainView<DecisionMakerItemViewModel, DecisionMakerSubView>
    {
        private DecisionContainerViewIntelligenceTab containerView;

        public DecisionMakerMainView() : base(UIBuilderResourcePaths.DecisionMakerMainView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            containerView = this.Q<DecisionContainerViewIntelligenceTab>();
        }

        protected override void OnInitSubView(DecisionMakerSubView subView)
        {
            containerView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionMakerItemViewModel viewModel)
        {
            containerView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            containerView.RootView = rootView;
        }
    }
}