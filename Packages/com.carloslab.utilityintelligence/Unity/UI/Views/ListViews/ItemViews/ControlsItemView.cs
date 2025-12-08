// <copyright file="ControlsItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.UtilityIntelligence.UI;

namespace CarlosLab.Common.UI
{
    public class ControlsItemView<TItemViewModel> : ControlsItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {

    }
}