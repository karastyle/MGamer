// <copyright file="TargetFilterItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using System;
using System.Reflection;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemCreatorView : NameTypeItemCreatorView<TargetFilterListViewModel, TargetFilterItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(TargetFilter);

        protected override string FormatListItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            var categoryAttribute = itemType.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute != null)
                return $"{categoryAttribute.Category}/{itemType.Name}";

            return itemType.Name;
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();

            string category1 = categoryAttribute1?.Category;
            string category2 = categoryAttribute2?.Category;

            if (string.IsNullOrWhiteSpace(category1) && string.IsNullOrWhiteSpace(category2))
                return string.CompareOrdinal(choice1.Name, choice2.Name);

            if (string.IsNullOrWhiteSpace(category1))
                return 1;

            if (string.IsNullOrWhiteSpace(category2))
                return -1;

            int result = string.CompareOrdinal(category1, category2);
            if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
            return result;
        }
    }
}