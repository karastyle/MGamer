// <copyright file="TargetFilterListViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewIntelligenceTab :
        MainBasicListView<TargetFilterListViewModelDecisionTab, TargetFilterItemViewModelDecisionTab, DecisionSubViewIntelligenceTab>
    {
        protected override void OnInitSubView(DecisionSubViewIntelligenceTab subView)
        {
            SubView.TargetFilterView.Hidden += ClearSelection;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowTargetFilterView(SelectedItem);
            else
                SubView.HideTargetFilterView();
        }

        protected override VisualElement MakeCellName()
        {
            return new TargetFilterNameItemViewIntelligenceTab();
        }

        protected override VisualElement MakeCellControls()
        {
            return new VisualElement();
        }
    }
}