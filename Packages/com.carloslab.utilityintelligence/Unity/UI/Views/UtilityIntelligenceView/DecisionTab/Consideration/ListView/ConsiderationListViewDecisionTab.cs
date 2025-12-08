// <copyright file="ConsiderationListViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationListViewDecisionTab
        : MainBasicListView<ConsiderationListViewModelDecisionTab, ConsiderationItemViewModelDecisionTab, DecisionSubView>
    {
        protected override void OnInitSubView(DecisionSubView subView)
        {
            SubView.ConsiderationView.Hidden += ClearSelection;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowConsiderationView(SelectedItem);
            else
                SubView.HideConsiderationView();
        }

        protected override VisualElement MakeCellName()
        {
            return new ConsiderationNameItemViewDecisionTab();
        }


    }
}