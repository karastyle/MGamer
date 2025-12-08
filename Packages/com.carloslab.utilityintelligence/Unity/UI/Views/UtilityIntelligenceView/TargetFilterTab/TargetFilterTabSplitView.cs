// <copyright file="TargetFilterTabSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class TargetFilterTabSplitView : SplitView<TargetFilterTabMainView, TargetFilterTabSubView>
    {
        public TargetFilterTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}