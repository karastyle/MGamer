// <copyright file="ActionItemCreatorViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using System;
using System.Reflection;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        ActionItemCreatorViewDecisionTab : BasicTypeItemCreatorView<ActionListViewModel, ActionItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(ActionTask);

        protected override string FormatListItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            var categoryAttribute = itemType.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute == null) return base.FormatListItem(itemType);

            return $"{categoryAttribute.Category}/{itemType.Name}";
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();

            if (categoryAttribute1 == null && categoryAttribute2 == null)
                return string.CompareOrdinal(choice1.Name, choice2.Name);

            if (categoryAttribute1 == null)
                return 1;

            if (categoryAttribute2 == null)
                return -1;

            int result = string.CompareOrdinal(categoryAttribute1.Category, categoryAttribute2.Category);
            if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
            return result;
        }
    }
}