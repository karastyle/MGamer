// <copyright file="ActionListViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionListViewDecisionTab : MainTypeListView<ActionListViewModel, ActionItemViewModel, DecisionSubView>
    {
        public ActionListViewDecisionTab()
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterControlsColumn(listView);
        }

        protected override void OnInitSubView(DecisionSubView subView)
        {
            SubView.ActionEditorView.Hidden += () => ClearSelection();
        }

        #region Make Cells

        protected override VisualElement MakeCellName()
        {
            return new ActionNameItemViewDecisionTab();
        }

        #endregion

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowActionEditorView(SelectedItem);
            else
                SubView.HideActionEditorView();
        }
    }
}