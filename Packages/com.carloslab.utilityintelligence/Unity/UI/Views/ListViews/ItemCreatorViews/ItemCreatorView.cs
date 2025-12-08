// <copyright file="ItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class ItemCreatorView<TListViewModel, TItemViewModel> : BaseItemCreatorView<TListViewModel, TItemViewModel, UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        protected ItemCreatorView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }

    public abstract class
        BasicTypeItemCreatorView<TListViewModel, TItemViewModel> : BasicTypeItemCreatorView<TListViewModel, TItemViewModel, UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {

    }

    public class
        NameTypeItemCreatorView<TListViewModel, TItemViewModel> : NameTypeItemCreatorView<TListViewModel, TItemViewModel
        , UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel,
        IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, INameViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
    }

    public abstract class
        NameItemCreatorView<TListViewModel, TItemViewModel> : NameItemCreatorView<TListViewModel, TItemViewModel,
        UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel,
        IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, INameViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {

    }
}