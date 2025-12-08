// <copyright file="ConsiderationTabSplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationTabSplitView : SplitView<ConsiderationTabMainView, ConsiderationTabSubView>
    {
        public ConsiderationTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}