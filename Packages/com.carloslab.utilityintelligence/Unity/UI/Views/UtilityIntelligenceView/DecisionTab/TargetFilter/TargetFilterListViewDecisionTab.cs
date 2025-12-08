// <copyright file="TargetFilterListViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewDecisionTab :
        MainBasicListView<TargetFilterListViewModelDecisionTab, TargetFilterItemViewModelDecisionTab, DecisionSubView>
    {
        protected override void OnInitSubView(DecisionSubView subView)
        {
            SubView.TargetFilterView.Hidden += ClearSelection;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowTargetFilterEditorView(SelectedItem);
            else
                SubView.HideTargetFilterEditorView();
        }

        protected override VisualElement MakeCellName()
        {
            return new TargetFilterNameItemViewDecisionTab();
        }
    }
}