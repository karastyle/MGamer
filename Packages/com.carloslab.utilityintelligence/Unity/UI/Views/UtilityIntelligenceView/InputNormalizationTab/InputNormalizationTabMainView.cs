// <copyright file="InputNormalizationTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationTabMainView : MainView<InputNormalizationListViewModel, InputNormalizationTabSubView>
    {
        private readonly InputNormalizationItemCreatorView itemCreatorView;
        private readonly InputNormalizationListView listView;

        public InputNormalizationTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Input Normalizations");
            Add(titleLabel);

            ScrollView scrollView = new();
            scrollView.style.marginLeft = 10;
            Add(scrollView);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            itemCreatorView = new();
            itemCreatorView.style.marginTop = 10;
            scrollView.Add(itemCreatorView);
        }

        protected override void OnInitSubView(InputNormalizationTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(InputNormalizationListViewModel viewModel)
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