// <copyright file="TargetNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetNameItemView<TItemViewModel> : BaseNameItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        public TargetNameItemView(bool enableRemove = true) : base(false, enableRemove)
        {
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            NameLabel.SetDataBinding(nameof(Label.text), nameof(ITargetViewModel.TargetName), BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            NameLabel.ClearBindings();
        }
    }
}