// <copyright file="GenericField.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using System;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class GenericField : RootViewValueField<UtilityIntelligenceView>
    {
        public GenericField(Type valueType, bool isDelayed = false, string label = null) : base(valueType, isDelayed,
            label)
        {
        }

        protected override VisualElement CreateField(Type valueType, bool isDelayed = false, string label = null)
        {
            VisualElement valueField = null;

            switch (valueType)
            {
                case Type type when typeof(IVariableReference).IsAssignableFrom(type) && type.IsGenericType:
                    VariableReferenceField variableReferenceFieldNew = new(label);
                    variableReferenceFieldNew.RootView = RootView;
                    variableReferenceFieldNew.ValueChanged += RaiseValueChanged;
                    variableReferenceFieldNew.InputApplied += RaiseInputApplied;
                    valueField = variableReferenceFieldNew;
                    break;
                // case Type type when type == typeof(InfluenceCurve):
                //     var influenceCurveField = new InfluenceCurveField(label);
                //     influenceCurveField.RegisterValueChangedCallback(evt =>
                //     {
                //         RaiseValueChanged(evt.newValue);
                //     });
                //     HandleInputApplied(influenceCurveField);
                //     valueField = influenceCurveField;
                //     break;
                default:
                    valueField = base.CreateField(valueType, isDelayed, label);
                    break;
            }

            return valueField;
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            if (ValueFieldConcrete is IRootViewMember<UtilityIntelligenceView> rootViewMemberField)
            {
                rootViewMemberField.RootView = rootView;
            }
        }
    }
}