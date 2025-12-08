// <copyright file="ResponseCurveViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ResponseCurveViewIntelligenceTab : UtilityIntelligenceViewMember<ConsiderationItemViewModelIntelligenceTab>
    {
        private readonly InfluenceCurveGraphView influenceCurveView;

        public ResponseCurveViewIntelligenceTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Response Curve";
            Add(foldout);

            influenceCurveView = new();
            influenceCurveView.style.alignSelf = Align.Center;
            foldout.Add(influenceCurveView);
        }

        protected override void OnRefreshView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            influenceCurveView.dataSource = viewModel.ContextViewModel;
            influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.InfluenceCurve),
                nameof(ConsiderationContextViewModel.ResponseCurve), BindingMode.ToTarget);
            influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.Input),
                nameof(ConsiderationContextViewModel.NormalizedInput), BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            influenceCurveView.ClearBindings();
        }
    }
}
