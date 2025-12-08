// <copyright file="DecisionListViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionListViewIntelligenceTab : MainTargetScoreListView<DecisionListViewModelIntelligenceTab, DecisionItemViewModelIntelligenceTab, DecisionMakerSubView>
    {
        public DecisionListViewIntelligenceTab()
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        protected override string TargetColumnTitle => "Best Target";

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowDecisionView(SelectedItem);
            else
                SubView.HideDecisionView();
        }

        protected override VisualElement MakeCellName()
        {
            return new DecisionNameItemViewIntelligenceTab();
        }

        protected override void BindCellTarget(VisualElement element, int index)
        {
            base.BindCellTarget(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }

        protected override void BindCellScore(VisualElement element, int index)
        {
            base.BindCellScore(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }
    }
}
