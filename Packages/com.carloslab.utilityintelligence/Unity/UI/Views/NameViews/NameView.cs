// <copyright file="NameView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameView<TViewModel>
        : NameView<TViewModel, UtilityIntelligenceView>
        where TViewModel : class, INameViewModel
    {
        protected NameView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }

    public abstract class NameMainView<TViewModel, TSubView>
        : NameMainView<TViewModel, TSubView, UtilityIntelligenceView>
        where TViewModel : class, INameViewModel
        where TSubView : BaseView, IRootViewMember
    {
        protected NameMainView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }
}