// <copyright file="DecisionTabSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionTabSplitView : SplitView<DecisionTabMainView, DecisionTabSubView>
    {
        public DecisionTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}