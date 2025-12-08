// <copyright file="DecisionMakerSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.UtilityIntelligence.Editor;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerSplitView : SplitView<DecisionMakerMainView, DecisionMakerSubView>
    {
        public DecisionMakerSplitView()
        {
            fixedPaneInitialDimension = FrameworkEditorPrefs.DecisionMakerSplitView_FixedPaneWidth;
        }

        protected override void FixedPane_OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!RootView.IsIntelligenceTabActive) return;
            FrameworkEditorPrefs.DecisionMakerSplitView_FixedPaneWidth = evt.newRect.width;
        }
    }
}