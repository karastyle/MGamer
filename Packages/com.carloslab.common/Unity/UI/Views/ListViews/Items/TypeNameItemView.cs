// <copyright file="TypeNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public abstract class TypeNameItemView<TItemViewModel, TRootView>
        : BaseNameItemView<TItemViewModel, TRootView>
        where TItemViewModel : class, IRootViewModelMember, ITypeNameViewModel, IItemViewModel
        where TRootView : BaseView, IRootView
    {
        protected TypeNameItemView() : base(false)
        {
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            NameLabel.text = viewModel.TypeName;
        }
    }
}