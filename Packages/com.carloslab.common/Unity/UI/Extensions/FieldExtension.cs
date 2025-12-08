// <copyright file="FieldExtension.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI.Extensions
{
    public static class FieldExtension
    {
        public static void RegisterInputAppliedCallback<TValueType>(this TextInputBaseField<TValueType> field, Action callback)
        {
            var inputs = field.Query(className: TextInputBaseField<object>.inputUssClassName).ToList();
            foreach (VisualElement input in inputs)
            {
                input.RegisterCallback<FocusOutEvent>(evt => callback?.Invoke());
            }

            var labels =
                field.Query(className: TextInputBaseField<object>.labelUssClassName).ToList();
            foreach (VisualElement label in labels)
            {
                label.RegisterCallback<MouseUpEvent>(evt => callback?.Invoke());
            }
        }

        public static void RegisterInputAppliedCallback<TValueType>(this UnityEngine.UIElements.BaseField<TValueType> field, Action callback)
        {
            var inputs = field.Query(className: BaseField<object>.inputUssClassName).ToList();
            foreach (VisualElement input in inputs)
            {
                input.RegisterCallback<FocusOutEvent>(evt => callback?.Invoke());
            }
        }

        public static void RegisterInputAppliedCallback<TValueType, TField, TFieldValue>(this BaseCompositeField<TValueType, TField, TFieldValue> field, Action callback)
            where TField : TextValueField<TFieldValue>, new()
        {
            var inputs = field.Query(className: TextInputBaseField<object>.inputUssClassName).ToList();
            foreach (VisualElement input in inputs)
            {
                input.RegisterCallback<FocusOutEvent>(evt => callback?.Invoke());
            }

            var labels =
                field.Query(className: TextInputBaseField<object>.labelUssClassName).ToList();
            foreach (VisualElement label in labels)
            {
                label.RegisterCallback<MouseUpEvent>(evt => callback?.Invoke());
            }
        }
    }
}