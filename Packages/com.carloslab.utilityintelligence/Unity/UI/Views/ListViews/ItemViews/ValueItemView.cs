// <copyright file="ValueItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ValueItemView<TViewModel> : ValueItemView<TViewModel, UtilityIntelligenceView>
        where TViewModel : class, IItemViewModel, IValueViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {
    }
}