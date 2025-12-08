// <copyright file="MainTypeListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class MainTypeListView<TListViewModel, TItemViewModel, TSubView> :
        MainBasicListView<TListViewModel, TItemViewModel, TSubView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TSubView : BaseView
    {
        protected override string NameColumnTitle => "Type";
    }

    public abstract class MainTypeTargetScoreListView<TListViewModel, TItemViewModel, TSubView> :
        MainTargetScoreListView<TListViewModel, TItemViewModel, TSubView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TSubView : BaseView
    {
        protected override string NameColumnTitle => "Type";
    }
}