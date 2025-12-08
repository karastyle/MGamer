// <copyright file="InfluenceCurveField.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    [UxmlElement]
    public partial class InfluenceCurveField : CustomField<InfluenceCurve>
    {
        private FloatField exponentField;
        private InfluenceCurveGraphView graphView;
        private InfluenceCurvePresetView presetView;
        private FloatField slopeField;
        private EnumField typeField;
        private FloatField xShiftField;
        private FloatField yShiftField;

        public InfluenceCurveField() : this(null)
        {
        }

        public InfluenceCurveField(string label) : base(label, UIBuilderResourcePaths.InfluenceCurveField)
        {

        }

        [CreateProperty]
        public float Input
        {
            get => graphView.Input;
            set => graphView.Input = value;
        }

        public event Action InputApplied;

        protected override void OnVisualAssetLoaded()
        {
            presetView = this.Q<InfluenceCurvePresetView>();
            presetView.PresetApplied += curve =>
            {
                if (value.Equals(curve)) return;

                value = curve;
                RaiseInputApplied();
            };

            graphView = this.Q<InfluenceCurveGraphView>();
            typeField = this.Q<EnumField>("TypeField");
            slopeField = this.Q<FloatField>("SlopeField");
            exponentField = this.Q<FloatField>("ExponentField");
            xShiftField = this.Q<FloatField>("XShiftField");
            yShiftField = this.Q<FloatField>("YShiftField");

            HandleBinding(typeField, nameof(InfluenceCurve.Type));
            typeField.RegisterValueChangedCallback(evt =>
            {
                InfluenceCurveType type = (InfluenceCurveType)evt.newValue;
                InfluenceCurve influenceCurve = this.value;
                influenceCurve.Type = type;
                value = influenceCurve;
                RaiseInputApplied();
            });

            HandleBinding(slopeField, nameof(InfluenceCurve.Slope));
            slopeField.RegisterInputAppliedCallback(RaiseInputApplied);

            HandleBinding(exponentField, nameof(InfluenceCurve.Exponent));
            exponentField.RegisterInputAppliedCallback(RaiseInputApplied);

            HandleBinding(xShiftField, nameof(InfluenceCurve.XShift));
            xShiftField.RegisterInputAppliedCallback(RaiseInputApplied);

            HandleBinding(yShiftField, nameof(InfluenceCurve.YShift));
            yShiftField.RegisterInputAppliedCallback(RaiseInputApplied);

            this.RegisterValueChangedCallback(evt =>
            {
                InfluenceCurve influenceCurve = evt.newValue;
                UpdateView(influenceCurve);
            });
        }

        private void HandleBinding(VisualElement field, string propertyName)
        {
            UpdateDataSource(field);
            field.SetDataBinding(nameof(value), propertyName, BindingMode.ToSource);
        }

        private void RaiseInputApplied()
        {
            InputApplied?.Invoke();
        }

        private void UpdateView(InfluenceCurve influenceCurve)
        {
            graphView.InfluenceCurve = influenceCurve;
            typeField.value = influenceCurve.Type;
            slopeField.value = influenceCurve.Slope;
            exponentField.value = influenceCurve.Exponent;
            xShiftField.value = influenceCurve.XShift;
            yShiftField.value = influenceCurve.YShift;
        }

        public void UpdateView(bool isRuntimeUI)
        {
            presetView.UpdateView(isRuntimeUI);
        }

        private void UpdateDataSource(VisualElement field)
        {
            field.dataSource = this;
            field.dataSourcePath = PropertyPath.FromName(nameof(value));
        }
    }
}