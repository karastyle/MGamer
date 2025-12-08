// <copyright file="BaseItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class BaseItemView<TItemViewModel> : BaseItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        public BaseItemView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }
}