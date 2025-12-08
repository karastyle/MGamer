// <copyright file="BlackboardTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class BlackboardTabMainView : MainView<BlackboardViewModel, BlackboardTabSubView>
    {
        private readonly VariableItemCreatorView itemCreatorView;
        private readonly VariableListView listView;

        public BlackboardTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Blackboard");
            Add(titleLabel);

            ScrollView scrollView = new();
            scrollView.style.marginLeft = 10;
            Add(scrollView);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;

            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new VariableListView();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            itemCreatorView = new VariableItemCreatorView();
            itemCreatorView.style.marginTop = 10;
            scrollView.Add(itemCreatorView);
        }

        protected override void OnInitSubView(BlackboardTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(BlackboardViewModel viewModel)
        {
            listView.UpdateView(viewModel);
            itemCreatorView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            itemCreatorView.RootView = rootView;
            listView.RootView = rootView;
        }
    }
}