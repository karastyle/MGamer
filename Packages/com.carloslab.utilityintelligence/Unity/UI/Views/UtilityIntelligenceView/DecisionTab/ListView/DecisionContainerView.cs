// <copyright file="DecisionContainerView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionContainerView : UtilityIntelligenceViewMember<DecisionListViewModel>, IMainView<DecisionTabSubView>
    {
        private DecisionItemCreatorView itemCreatorView;
        private DecisionListView listView;
        public DecisionListView ListView => listView;

        public DecisionContainerView() : base(null)
        {
            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            Add(reorderableToggle);

            listView = new();
            listView.style.marginTop = 5;
            Add(listView);

            itemCreatorView = new();
            itemCreatorView.style.marginTop = 10;
            Add(itemCreatorView);

            style.marginLeft = 10;
        }

        public DecisionTabSubView SubView { get; private set; }

        public void InitSubView(DecisionTabSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionListViewModel viewModel)
        {
            listView.UpdateView(viewModel);
            itemCreatorView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
            itemCreatorView.RootView = rootView;
        }
    }
}