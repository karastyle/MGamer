// <copyright file="ControlsItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public class ControlsItemView<TItemViewModel, TRootView> : BaseItemView<TItemViewModel, TRootView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        public ControlsItemView() : base(null)
        {
            CreateRemoveButton();
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
        }
    }
}