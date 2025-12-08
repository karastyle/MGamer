// <copyright file="DecisionTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionTabMainView : MainView<DecisionListViewModel, DecisionTabSubView>
    {
        private DecisionContainerView containerView;
        public DecisionContainerView ContainerView => containerView;

        public DecisionTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Decisions");
            Add(titleLabel);

            ScrollView scrollView = new();
            Add(scrollView);

            containerView = new();

            scrollView.Add(containerView);
        }

        protected override void OnInitSubView(DecisionTabSubView subView)
        {
            containerView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionListViewModel viewModel)
        {
            containerView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            containerView.RootView = rootView;
        }
    }
}