// <copyright file="InputNormalizationListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationListView : MainBasicListView<InputNormalizationListViewModel, InputNormalizationItemViewModel, InputNormalizationTabSubView>
    {
        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowInputEditorView(SelectedItem);
            else
                SubView.HideInputEditorView();
        }

        #region Make Cells

        protected override VisualElement MakeCellName()
        {
            return new InputNormalizationNameItemView();
        }

        #endregion
    }
}