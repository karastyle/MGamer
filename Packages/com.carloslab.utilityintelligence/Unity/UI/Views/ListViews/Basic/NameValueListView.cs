// <copyright file="NameValueListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameValueListView<TListViewModel, TItemViewModel> :
        NameValueListView<TListViewModel, TItemViewModel, UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, INameViewModel, IValueViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {

    }
}