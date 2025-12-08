// <copyright file="VisualElementExtension.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI.Extensions
{
    public static class VisualElementExtension
    {
        public static void SetDisplay(this VisualElement element, bool display)
        {
            element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static bool IsDisplaying(this VisualElement element)
        {
            return element.style.display == DisplayStyle.Flex;
        }

        public static void SetVisibility(this VisualElement element, bool visibility)
        {
            element.style.visibility = visibility ? Visibility.Visible : Visibility.Hidden;
        }

        public static bool IsVisible(this VisualElement element)
        {
            return element.style.visibility == Visibility.Visible;
        }

        public static VisualElement GetFirstAncestorWithClass(this VisualElement element, string className)
        {
            if (element == null)
                return null;

            if (element.ClassListContains(className))
                return element;

            return element.parent.GetFirstAncestorWithClass(className);
        }

        public static void SetDataBinding(this VisualElement element, string targetPropertyName,
            string sourcePropertyName, BindingMode bindingMode,
            BindingUpdateTrigger updateTrigger = BindingUpdateTrigger.OnSourceChanged)
        {
            element.SetBinding(targetPropertyName, new DataBinding
            {
                bindingMode = bindingMode,
                updateTrigger = updateTrigger,
                dataSourcePath = PropertyPath.FromName(sourcePropertyName)
            });
        }
    }
}