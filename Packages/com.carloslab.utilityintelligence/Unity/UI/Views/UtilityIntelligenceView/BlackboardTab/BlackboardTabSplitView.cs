// <copyright file="BlackboardTabSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class BlackboardTabSplitView : SplitView<BlackboardTabMainView, BlackboardTabSubView, UtilityIntelligenceView>
    {
        public BlackboardTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}