// <copyright file="ContainerViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ContainerViewModel<TModel, TItemModel, TItemViewModel>
        : ContainerViewModel<TModel, TItemModel, TItemViewModel, UtilityIntelligenceViewModel>
        where TModel : class, IItemContainerModel<TItemModel>
        where TItemModel : class, IModelWithId, IContainerItem
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, INameViewModel
        , IRootViewModelMember<UtilityIntelligenceViewModel>, new()
    {

    }
}