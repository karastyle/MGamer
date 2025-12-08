// <copyright file="NameListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameListViewModel<TContainerModel, TItemModel, TItemViewModel>
        : NameListViewModel<TContainerModel, TItemModel, TItemViewModel, UtilityIntelligenceViewModel>
        where TContainerModel : class, IModel
        where TItemModel : class, IModelWithId, IContainerItem
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, INameViewModel
        , IRootViewModelMember<UtilityIntelligenceViewModel>, new()
    {

    }
}