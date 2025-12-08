// <copyright file="IntelligenceTabSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using CarlosLab.UtilityIntelligence.Editor;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class IntelligenceTabSplitView : SplitView<IntelligenceTabMainView, IntelligenceTabSubView, UtilityIntelligenceView>
    {
        public IntelligenceTabSplitView()
        {
            fixedPaneInitialDimension = FrameworkEditorPrefs.IntelligenceSplitView_FixedPaneWidth;
        }

        protected override void FixedPane_OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!RootView.IsIntelligenceTabActive) return;
            FrameworkEditorPrefs.IntelligenceSplitView_FixedPaneWidth = evt.newRect.width;
        }
    }
}