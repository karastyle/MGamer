// <copyright file="InputTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputTabMainView : MainView<InputListViewModel, InputTabSubView>
    {
        private readonly InputItemCreatorView itemCreatorView;
        private readonly InputListView listView;

        public InputTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Inputs");
            Add(titleLabel);

            ScrollView scrollView = new();
            scrollView.style.marginLeft = 10;
            Add(scrollView);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new InputListView();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            itemCreatorView = new InputItemCreatorView();
            itemCreatorView.style.marginTop = 10;
            scrollView.Add(itemCreatorView);
        }

        protected override void OnInitSubView(InputTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(InputListViewModel viewModel)
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