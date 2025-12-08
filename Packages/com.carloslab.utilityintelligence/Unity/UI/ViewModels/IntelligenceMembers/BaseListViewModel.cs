// <copyright file="BaseListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class BaseListViewModel<TModel, TItemModel, TItemViewModel> :
        BaseListViewModel<TModel, TItemModel, TItemViewModel, UtilityIntelligenceViewModel>
        where TModel : class, IModel
        where TItemModel : class, IModelWithId
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, IRootViewModelMember<UtilityIntelligenceViewModel>, new()
    {


    }
}