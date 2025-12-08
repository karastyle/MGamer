// <copyright file="VariableItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using System;
using System.Reflection;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableItemCreatorView : NameTypeItemCreatorView<BlackboardViewModel, VariableViewModel>
    {
        protected override Type BaseType { get; } = typeof(Variable);

        protected override string FormatListItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            string typeName = itemType.Name.Replace("Variable", string.Empty);

            var categoryAttribute = itemType.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute != null)
                return $"{categoryAttribute.Category}/{typeName}";

            return typeName;
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var typeName1 = choice1.Name.Replace("Variable", string.Empty);
            var typeName2 = choice2.Name.Replace("Variable", string.Empty);

            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();

            if (categoryAttribute1 == null && categoryAttribute2 == null)
                return string.CompareOrdinal(typeName1, typeName2);

            if (categoryAttribute1 == null)
                return 1;

            if (categoryAttribute2 == null)
                return -1;

            int result = string.CompareOrdinal(categoryAttribute1.Category, categoryAttribute2.Category);
            if (result == 0) return string.CompareOrdinal(typeName1, typeName2);
            return result;
        }

        protected override string FormatSelectedItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            return itemType.Name.Replace("Variable", string.Empty);
        }
    }
}