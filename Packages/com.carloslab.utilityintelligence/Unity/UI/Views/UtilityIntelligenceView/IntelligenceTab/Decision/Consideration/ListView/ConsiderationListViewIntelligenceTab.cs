// <copyright file="ConsiderationListViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationListViewIntelligenceTab
        : MainTargetScoreListView<ConsiderationListViewModelIntelligenceTab, ConsiderationItemViewModelIntelligenceTab, DecisionSubViewIntelligenceTab>
    {

        // protected override string ScoreColumnTitle => "Compensated Score";
        //
        // protected override float ScoreColumnWidth => 130;

        public ConsiderationListViewIntelligenceTab()
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        protected override void OnInitSubView(DecisionSubViewIntelligenceTab subView)
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
            return new ConsiderationNameItemViewIntelligenceTab();
        }

        protected override VisualElement MakeScoreCell()
        {
            ConsiderationScoreItemViewIntelligenceTab scoreCell = new();
            scoreCell.tooltip = "Compensated Score";
            return scoreCell;
        }

        protected override VisualElement MakeTargetCell()
        {
            ConsiderationTargetItemViewIntelligenceTab targetCell = new();
            return targetCell;
        }

        protected override VisualElement MakeCellControls()
        {
            return new VisualElement();
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