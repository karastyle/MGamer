// <copyright file="ResponseCurveView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ResponseCurveView : UtilityIntelligenceViewMember<ConsiderationItemViewModel>
    {
        private readonly InfluenceCurveField responseCurveField;

        public ResponseCurveView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Response Curve";
            Add(foldout);

            responseCurveField = new();
            responseCurveField.style.alignSelf = Align.Center;
            responseCurveField.InputApplied += OnResponseCurveFieldInputApplied;
            foldout.Add(responseCurveField);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            responseCurveField.UpdateView(IsRuntimeUI);
        }

        protected override void OnModelChanged(IModel newModel)
        {
            //TODO Refactor UI
            // responseCurveField.value = ViewModel.ResponseCurve;
        }

        private void OnResponseCurveFieldInputApplied()
        {
            ViewModel.Record($"ResponseCurveView ResponseCurve Changed: {responseCurveField.value.ToString()}",
                () => { ViewModel.ResponseCurve = responseCurveField.value; });
        }

        protected override void OnRefreshView(ConsiderationItemViewModel viewModel)
        {
            responseCurveField.SetDataBinding(nameof(InfluenceCurveField.value),
                nameof(ConsiderationItemViewModel.ResponseCurve), BindingMode.ToTarget);
        }

        protected override void OnEnableEditMode()
        {
            responseCurveField.SetDataBinding(nameof(InfluenceCurveField.Input),
                nameof(ConsiderationItemViewModel.NormalizedInput), BindingMode.ToTarget);
        }

        protected override void OnEnableRuntimeMode()
        {
            // responseCurveField.SetEnabled(false);
        }

        protected override void OnResetView()
        {
            responseCurveField.ClearBindings();
        }
    }
}