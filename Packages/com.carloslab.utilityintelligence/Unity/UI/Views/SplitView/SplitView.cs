// <copyright file="SplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class SplitView<TMainView, TSubView> : SplitView<TMainView, TSubView, UtilityIntelligenceView>
        where TMainView : BaseView, IMainView<TSubView>, IRootViewMember<UtilityIntelligenceView>, new()
        where TSubView : BaseView, IRootViewMember<UtilityIntelligenceView>, new()
    {

    }
}