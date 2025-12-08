// <copyright file="MainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class MainView<TViewModel, TSubView> : MainView<TViewModel, TSubView, UtilityIntelligenceView>
        where TViewModel : class, IViewModel
        where TSubView : BaseView, IRootViewMember
    {
        public MainView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }
}