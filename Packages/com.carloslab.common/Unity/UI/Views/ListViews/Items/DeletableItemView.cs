// <copyright file="DeletableItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public abstract class DeletableItemView<TItemViewModel, TRootView> : BaseItemView<TItemViewModel, TRootView>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel
        where TRootView : BaseView, IRootView
    {
        public DeletableItemView() : base(null)
        {
            CreateRemoveButton();
        }
    }
}