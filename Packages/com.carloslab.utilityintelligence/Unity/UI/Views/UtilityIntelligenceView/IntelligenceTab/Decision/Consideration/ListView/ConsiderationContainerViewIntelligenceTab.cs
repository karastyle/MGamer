// <copyright file="ConsiderationContainerViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationContainerViewIntelligenceTab : UtilityIntelligenceViewMember<ConsiderationListViewModelIntelligenceTab>, IMainView<DecisionSubViewIntelligenceTab>
    {
        private readonly ConsiderationListViewIntelligenceTab listView;

        public ConsiderationContainerViewIntelligenceTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Considerations";
            Add(foldout);

            VisualElement container = new();
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

        protected override void OnUpdateView(ConsiderationListViewModelIntelligenceTab viewModel)
        {
            listView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
        }
    }
}
