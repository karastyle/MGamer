// <copyright file="ConsiderationListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationListView : MainBasicListView<ConsiderationListViewModel,
        ConsiderationItemViewModel, ConsiderationTabSubView>
    {

        #region Make Cells

        protected override VisualElement MakeCellName()
        {
            return new ConsiderationNameItemView();
        }

        #endregion

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowEditorView(SelectedItem);
            else
                SubView.HideEditorView();
        }
    }
}