// <copyright file="BaseItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class BaseItemViewModel<TItemModel, TListViewModel> : BaseItemViewModel<TItemModel, TListViewModel, UtilityIntelligenceViewModel>
        where TItemModel : class, IModelWithId
        where TListViewModel : class, IListViewModel
    {
    }
}