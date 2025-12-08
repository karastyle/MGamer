// <copyright file="DecisionSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionSplitView : SplitView<DecisionMainView, DecisionSubView>
    {
        public DecisionSplitView()
        {
            MainView.style.minWidth = 256;
            // fixedPaneInitialDimension = 400;
        }
    }
}