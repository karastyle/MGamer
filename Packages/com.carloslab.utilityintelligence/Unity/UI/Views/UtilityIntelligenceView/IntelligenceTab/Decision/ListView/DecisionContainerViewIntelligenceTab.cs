// <copyright file="DecisionContainerViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionContainerViewIntelligenceTab : UtilityIntelligenceViewMember<DecisionMakerItemViewModel>, IMainView<DecisionMakerSubView>
    {
        private DecisionItemCreatorViewIntelligenceTab itemCreatorView;
        private DecisionListViewIntelligenceTab listView;

        public DecisionContainerViewIntelligenceTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Decisions";
            Add(foldout);

            VisualElement container = new();
            foldout.Add(container);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            container.Add(reorderableToggle);

            listView = new();
            listView.style.marginTop = 5;
            container.Add(listView);

            itemCreatorView = new();
            itemCreatorView.style.marginTop = 10;
            container.Add(itemCreatorView);
        }

        public DecisionMakerSubView SubView { get; private set; }
        public void InitSubView(DecisionMakerSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionMakerItemViewModel viewModel)
        {
            listView.UpdateView(viewModel?.DecisionListViewModel);
            itemCreatorView.UpdateView(viewModel?.DecisionListViewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
            itemCreatorView.RootView = rootView;
        }
    }
}