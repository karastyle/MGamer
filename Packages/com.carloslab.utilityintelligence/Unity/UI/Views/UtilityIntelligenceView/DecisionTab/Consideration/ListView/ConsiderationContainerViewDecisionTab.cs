// <copyright file="ConsiderationContainerViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationContainerViewDecisionTab : UtilityIntelligenceViewMember<ConsiderationListViewModelDecisionTab>, IMainView<DecisionSubView>
    {
        private readonly ConsiderationItemCreatorViewDecisionTab itemCreatorView;
        private readonly ConsiderationListViewDecisionTab listView;

        public ConsiderationContainerViewDecisionTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Considerations";
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


        public DecisionSubView SubView { get; private set; }

        public void InitSubView(DecisionSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(ConsiderationListViewModelDecisionTab viewModel)
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