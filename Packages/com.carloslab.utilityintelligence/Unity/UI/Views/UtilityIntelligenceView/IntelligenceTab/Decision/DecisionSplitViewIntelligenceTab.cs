// <copyright file="DecisionSplitViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.UtilityIntelligence.Editor;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionSplitViewIntelligenceTab : SplitView<DecisionMainViewIntelligenceTab, DecisionSubViewIntelligenceTab>
    {
        public DecisionSplitViewIntelligenceTab()
        {
            fixedPaneInitialDimension = FrameworkEditorPrefs.DecisionSplitView_FixedPaneWidth;
        }

        protected override void FixedPane_OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!RootView.IsIntelligenceTabActive) return;
            FrameworkEditorPrefs.DecisionSplitView_FixedPaneWidth = evt.newRect.width;
        }
    }
}