// <copyright file="TypeNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TypeNameItemView<TItemViewModel>
        : TypeNameItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, ITypeNameViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {

    }
}