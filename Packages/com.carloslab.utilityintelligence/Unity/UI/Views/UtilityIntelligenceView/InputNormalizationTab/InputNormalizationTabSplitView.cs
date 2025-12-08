// <copyright file="InputNormalizationTabSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationTabSplitView : SplitView<InputNormalizationTabMainView, InputNormalizationTabSubView>
    {
        public InputNormalizationTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}