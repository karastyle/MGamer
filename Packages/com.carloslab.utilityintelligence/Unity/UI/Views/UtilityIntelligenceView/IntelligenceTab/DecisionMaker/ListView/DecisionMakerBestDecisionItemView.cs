// <copyright file="DecisionMakerBestDecisionItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerBestDecisionItemView : BaseNameItemView<DecisionMakerItemViewModel>
    {
        public DecisionMakerBestDecisionItemView() : base(false)
        {
        }

        protected override void OnRefreshView(DecisionMakerItemViewModel viewModel)
        {
            NameLabel.dataSource = viewModel.ContextViewModel;
            NameLabel.SetDataBinding(nameof(Label.text), nameof(DecisionMakerContextViewModel.BestDecisionName),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            NameLabel.ClearBindings();
        }
    }
}