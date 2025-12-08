// <copyright file="DecisionListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionListView
        : MainBasicListView<DecisionListViewModel, DecisionItemViewModel, DecisionTabSubView>
    {
        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowDecisionView(SelectedItem);
            else
                SubView.HideDecisionView();
        }

        #region Make/Bind Cells

        protected override VisualElement MakeCellName()
        {
            return new DecisionNameItemView();
        }

        #endregion
    }
}