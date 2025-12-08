// <copyright file="ResponseCurveViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ResponseCurveViewDecisionTab : UtilityIntelligenceViewMember<ConsiderationItemViewModelDecisionTab>
    {
        private readonly InfluenceCurveGraphView influenceCurveView;

        public ResponseCurveViewDecisionTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Response Curve";
            Add(foldout);

            influenceCurveView = new();
            influenceCurveView.style.alignSelf = Align.Center;
            foldout.Add(influenceCurveView);
        }

        protected override void OnRefreshView(ConsiderationItemViewModelDecisionTab viewModel)
        {
            influenceCurveView.dataSource = viewModel.ConsiderationViewModel;
            influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.InfluenceCurve),
                nameof(ConsiderationItemViewModel.ResponseCurve), BindingMode.ToTarget);
        }

        protected override void OnEnableEditMode()
        {
            influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.Input),
                nameof(ConsiderationItemViewModel.NormalizedInput), BindingMode.ToTarget);
        }

        protected override void OnEnableRuntimeMode()
        {
            influenceCurveView.SetEnabled(false);
        }

        protected override void OnResetView()
        {
            influenceCurveView.ClearBindings();
        }
    }
}