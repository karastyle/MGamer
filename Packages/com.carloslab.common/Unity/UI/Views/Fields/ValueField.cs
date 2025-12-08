// <copyright file="ValueField.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Reflection;
using CarlosLab.Common.Extensions;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace CarlosLab.Common.UI
{
    public class ValueField : VisualElement
    {
        public ValueField(Type valueType, bool isDelayed = false, string label = null)
        {
            valueFieldConcrete = CreateField(valueType, isDelayed, label);
            if (valueFieldConcrete != null)
            {
                Type fieldType = valueFieldConcrete.GetType();
                setValueWithoutNotify = fieldType.GetMethod(nameof(BaseField<object>.SetValueWithoutNotify));
                valueProperty = fieldType.GetProperty(nameof(BaseField<object>.value));
                Add(valueFieldConcrete);
            }
        }

        public event Action<object> ValueChanged;
        public event Action InputApplied;

        protected void RaiseValueChanged(object newValue)
        {
            ValueChanged?.Invoke(newValue);
        }

        protected void RaiseInputApplied()
        {
            InputApplied?.Invoke();
        }

        #region Fields

        private readonly VisualElement valueFieldConcrete;

        private readonly MethodInfo setValueWithoutNotify;
        private readonly PropertyInfo valueProperty;

        #endregion

        #region Properties

        public VisualElement ValueFieldConcrete => valueFieldConcrete;

        public bool IsValid => valueFieldConcrete != null;

        public object Value
        {
            get
            {
                if (valueProperty == null) return null;

#if UNITY_EDITOR
                if (valueFieldConcrete is LayerMaskField layerMaskField)
                {
                    LayerMask layerMask = layerMaskField.value;
                    return layerMask;
                }
                else
                {
                    return valueProperty.GetValue(valueFieldConcrete);
                }
#else
                return valueProperty.GetValue(valueFieldConcrete);
#endif
            }
            set
            {
                if (valueProperty == null) return;

#if UNITY_EDITOR
                if (valueFieldConcrete is LayerMaskField)
                {
                    if (value is LayerMask layerMask)
                    {
                        int layerMaskInt = layerMask;
                        valueProperty.SetValue(valueFieldConcrete, layerMaskInt);
                    }
                    else
                    {
                        Debug.LogError("ValueField: SetValue LayerMask Error");
                    }
                }
                else
                {
                    valueProperty.SetValue(valueFieldConcrete, value);
                }
#else
                valueProperty.SetValue(valueFieldConcrete, value);
#endif
            }
        }

        #endregion

        #region Value Functions

        public void SetValueWithoutNotify(object value)
        {
            setValueWithoutNotify?.Invoke(valueFieldConcrete, new[] { value });
        }

        public void SetValueDataBinding(string sourcePropertyName, BindingMode bindingMode,
            BindingUpdateTrigger updateTrigger = BindingUpdateTrigger.OnSourceChanged)
        {
            valueFieldConcrete?.SetDataBinding(nameof(BaseField<object>.value), sourcePropertyName, bindingMode, updateTrigger);
        }

        #endregion

        #region UI Functions

        protected virtual VisualElement CreateField(Type valueType, bool isDelayed = false, string label = null)
        {
            VisualElement valueField = null;

            switch (valueType)
            {
                case { } type when type == typeof(int):
                    IntegerField integerField = new(label);
                    integerField.isDelayed = isDelayed;
                    integerField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    integerField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = integerField;
                    break;
                case { } type when type == typeof(long):
                    LongField longField = new(label);
                    longField.isDelayed = isDelayed;
                    longField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    longField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = longField;
                    break;
                case { } type when type == typeof(float):
                    FloatField floatField = new(label);
                    floatField.isDelayed = isDelayed;
                    floatField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    floatField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = floatField;
                    break;
                case { } type when type == typeof(double):
                    DoubleField doubleField = new(label);
                    doubleField.isDelayed = isDelayed;
                    doubleField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    doubleField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = doubleField;
                    break;
                case { } type when type == typeof(string):
                    TextField textField = new(label);
                    textField.isDelayed = isDelayed;
                    textField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    textField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = textField;
                    break;
                case { } type when type == typeof(Float2) || type == typeof(Vector2):
                    Vector2Field vector2Field = new(label);
                    vector2Field.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    vector2Field.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = vector2Field;
                    break;
                case { } type when type == typeof(Float3) || type == typeof(Vector3):
                    Vector3Field vector3Field = new(label);
                    vector3Field.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    vector3Field.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = vector3Field;
                    break;
                case { } type when type == typeof(Int2) || type == typeof(Vector2Int):
                    Vector2IntField vector2IntField = new(label);
                    vector2IntField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    vector2IntField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = vector2IntField;
                    break;
                case { } type when type == typeof(Int3) || type == typeof(Vector3Int):
                    Vector3IntField vector3IntField = new(label);
                    vector3IntField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    vector3IntField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = vector3IntField;
                    break;
                case { } type when type == typeof(bool):
                    Toggle toggleField = new(label);
                    toggleField.RegisterValueChangedCallback(evt =>
                    {
                        RaiseValueChanged(evt.newValue);
                        RaiseInputApplied();
                    });
                    valueField = toggleField;
                    break;
#if UNITY_EDITOR
                case { } type when type == typeof(Color):
                    ColorField colorField = new(label);
                    colorField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    colorField.RegisterInputAppliedCallback(RaiseInputApplied);
                    valueField = colorField;
                    break;
                case { } type when type == typeof(LayerMask):
                    LayerMaskField layerMaskField = new(label);
                    layerMaskField.RegisterValueChangedCallback(evt =>
                    {
                        RaiseValueChanged(evt.newValue);
                        RaiseInputApplied();
                    });
                    valueField = layerMaskField;
                    break;
#endif
                case { IsEnum: true } type:
                    Enum defaultValue = (Enum)Enum.ToObject(type, 0);
#if UNITY_EDITOR
                    if (type.HasAttribute<FlagsAttribute>())
                    {
                        EnumFlagsField enumFlagsField = new(label, defaultValue);
                        enumFlagsField.RegisterValueChangedCallback(evt =>
                        {
                            RaiseValueChanged(evt.newValue);
                            RaiseInputApplied();
                        });
                        valueField = enumFlagsField;
                        break;
                    }
#endif

                    EnumField enumField = new(label, defaultValue);
                    enumField.RegisterValueChangedCallback(evt =>
                    {
                        RaiseValueChanged(evt.newValue);
                        RaiseInputApplied();
                    });
                    valueField = enumField;

                    break;
            }

            return valueField;
        }

        public void ClearMarginLeft()
        {
            valueFieldConcrete.style.marginLeft = 0;
        }

        #endregion
    }
}