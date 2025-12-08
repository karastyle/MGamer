// <copyright file="TargetFilterContainerViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class TargetFilterContainerViewDecisionTab : UtilityIntelligenceViewMember<DecisionItemViewModel>, IMainView<DecisionSubView>
    {
        private readonly VisualElement container;

        private TargetFilterListViewDecisionTab listView;
        private TargetFilterItemCreatorViewDecisionTab itemCreatorView;

        public TargetFilterContainerViewDecisionTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Target Filters";
            Add(foldout);

            container = new();

            CreateReorderableToggle(container);

            CreateListView(container);

            foldout.Add(container);
        }

        private void CreateListView(VisualElement container)
        {
            listView = new();
            listView.style.marginTop = 5;
            container.Add(listView);

            itemCreatorView = new();
            itemCreatorView.style.marginTop = 10;
            container.Add(itemCreatorView);
        }

        private void CreateReorderableToggle(VisualElement container)
        {
            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            container.Add(reorderableToggle);
        }

        public DecisionSubView SubView { get; private set; }

        public void InitSubView(DecisionSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionItemViewModel viewModel)
        {
            listView.UpdateView(viewModel?.TargetFilterListViewModel);
            itemCreatorView.UpdateView(viewModel?.TargetFilterListViewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
            itemCreatorView.RootView = rootView;
        }
    }
}