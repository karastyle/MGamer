// <copyright file="InfluenceCurvePresetView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    [UxmlElement]
    public partial class InfluenceCurvePresetView : BaseView
    {
        private readonly Button applyButton;
        private readonly DropdownField presetField;

        private readonly Dictionary<string, InfluenceCurve> presets = new()
        {
            { "Basic Linear", InfluenceCurve.BasicLinear },
            { "Inverse Linear", InfluenceCurve.InverseLinear },

            { "Constant", InfluenceCurve.Constant },
            { "Basic logistic", InfluenceCurve.BasicLogistic },
            { "Inverse logistic", InfluenceCurve.InverseLogistic },
            { "Basic logit", InfluenceCurve.BasicLogit },
            { "Inverse logit", InfluenceCurve.InverseLogit },
            { "Basic quadric lower left", InfluenceCurve.BasicQuadricLowerLeft },
            { "Basic quadric lower right", InfluenceCurve.BasicQuadricLowerRight },
            { "Basic quadric upper left", InfluenceCurve.BasicQuadricUpperLeft },
            { "Basic quadric upper right", InfluenceCurve.BasicQuadricUpperRight },
            { "Basic sine", InfluenceCurve.BasicSine },
            { "Inverse sine", InfluenceCurve.InverseSine },
            { "Basic bell curve", InfluenceCurve.BasicBellCurve },
            { "Inverse bell curve", InfluenceCurve.InverseBellCurve },
        };

        public InfluenceCurvePresetView() : base(null)
        {
            style.alignItems = Align.Center;
            presetField = new();
            presetField.style.minWidth = 200;
            Add(presetField);

            UpdatePresetFieldChoices();

            applyButton = new Button();
            applyButton.text = "Apply";
            applyButton.clicked += () => PresetApplied?.Invoke(presets[presetField.value]);
            applyButton.style.marginTop = 5;
            Add(applyButton);
        }

        public event Action<InfluenceCurve> PresetApplied;

        private void UpdatePresetFieldChoices()
        {
            foreach (KeyValuePair<string, InfluenceCurve> preset in presets)
            {
                presetField.choices.Add(preset.Key);
            }

            presetField.SetValueWithoutNotify(presetField.choices[0]);
        }

        public void UpdateView(bool isRuntimeUI)
        {
            if (isRuntimeUI)
                style.marginBottom = 200;
        }
    }
}