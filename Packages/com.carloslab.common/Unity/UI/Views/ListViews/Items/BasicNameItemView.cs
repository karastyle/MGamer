// <copyright file="BasicNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class BasicNameItemView<TItemViewModel, TRootView> : BaseNameItemView<TItemViewModel, TRootView>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel, INameViewModel
        where TRootView : BaseView, IRootView
    {
        public BasicNameItemView(bool enableRename, bool enableRemove = true) : base(enableRename, enableRemove)
        {
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            NameLabel.SetDataBinding(nameof(Label.text), nameof(INameViewModel.Name), BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            NameLabel.ClearBindings();
        }
    }
}