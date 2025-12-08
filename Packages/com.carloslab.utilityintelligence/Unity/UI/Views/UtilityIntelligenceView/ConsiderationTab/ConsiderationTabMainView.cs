// <copyright file="ConsiderationTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationTabMainView : MainView<ConsiderationListViewModel, ConsiderationTabSubView>
    {
        private readonly ConsiderationItemCreatorView creatorView;
        private readonly ConsiderationListView listView;
        public ConsiderationListView ListView => listView;

        public ConsiderationTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Considerations");
            Add(titleLabel);

            ScrollView scrollView = new();
            scrollView.style.marginLeft = 10;
            Add(scrollView);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new ConsiderationListView();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            creatorView = new ConsiderationItemCreatorView();
            creatorView.style.marginTop = 10;
            scrollView.Add(creatorView);
        }

        protected override void OnInitSubView(ConsiderationTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(ConsiderationListViewModel viewModel)
        {
            listView.UpdateView(viewModel);
            creatorView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
            creatorView.RootView = rootView;
        }
    }
}