// <copyright file="DecisionMakerContainerView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerContainerView : UtilityIntelligenceViewMember<DecisionMakerListViewModel>, IMainView<IntelligenceTabSubView>
    {
        private readonly DecisionMakerItemCreatorView itemCreatorView;
        private readonly DecisionMakerListView listView;

        public DecisionMakerContainerView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Decision Makers";
            Add(foldout);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;

            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            foldout.Add(reorderableToggle);

            listView = new DecisionMakerListView();
            listView.style.marginTop = 5;
            foldout.Add(listView);

            itemCreatorView = new DecisionMakerItemCreatorView();
            itemCreatorView.style.marginTop = 10;
            foldout.Add(itemCreatorView);
        }

        public IntelligenceTabSubView SubView { get; private set; }

        public void InitSubView(IntelligenceTabSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionMakerListViewModel viewModel)
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