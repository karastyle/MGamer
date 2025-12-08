// <copyright file="TargetFilterTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterTabMainView : MainView<TargetFilterListViewModel, TargetFilterTabSubView>
    {
        private readonly TargetFilterItemCreatorView creatorView;
        private readonly TargetFilterListView listView;
        public TargetFilterListView ListView => listView;

        public TargetFilterTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("TargetFilters");
            Add(titleLabel);

            ScrollView scrollView = new();
            scrollView.style.marginLeft = 10;
            Add(scrollView);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new TargetFilterListView();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            creatorView = new TargetFilterItemCreatorView();
            creatorView.style.marginTop = 10;
            scrollView.Add(creatorView);
        }

        protected override void OnInitSubView(TargetFilterTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(TargetFilterListViewModel viewModel)
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