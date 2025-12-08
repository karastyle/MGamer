// <copyright file="TargetFilterContainerViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class TargetFilterContainerViewIntelligenceTab : UtilityIntelligenceViewMember<DecisionItemViewModel>, IMainView<DecisionSubViewIntelligenceTab>
    {
        private readonly VisualElement container;

        private readonly Toggle hasNoTargetToggle;
        private readonly TargetFilterListViewIntelligenceTab listView;

        public TargetFilterContainerViewIntelligenceTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Target Filters";
            Add(foldout);

            hasNoTargetToggle = new("Has No Target");
            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                container.SetDisplay(!hasNoTarget);
            });
            hasNoTargetToggle.SetEnabled(false);
            foldout.Add(hasNoTargetToggle);

            container = new();
            foldout.Add(container);

            listView = new();
            listView.style.marginTop = 5;
            container.Add(listView);
        }

        public DecisionSubViewIntelligenceTab SubView { get; private set; }

        public void InitSubView(DecisionSubViewIntelligenceTab subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionItemViewModel viewModel)
        {
            listView.UpdateView(viewModel?.TargetFilterListViewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
        }

        protected override void OnRefreshView(DecisionItemViewModel viewModel)
        {
            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(DecisionItemViewModel.HasNoTarget),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            hasNoTargetToggle.ClearBindings();
        }
    }
}