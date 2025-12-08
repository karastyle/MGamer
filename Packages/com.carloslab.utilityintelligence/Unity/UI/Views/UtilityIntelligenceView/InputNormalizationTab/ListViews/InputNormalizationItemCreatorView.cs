// <copyright file="InputNormalizationItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using CarlosLab.Common.Extensions;
using System;
using System.Reflection;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationItemCreatorView : NameTypeItemCreatorView<InputNormalizationListViewModel, InputNormalizationItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(InputNormalization);

        protected override string FormatListItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            Type inputValueType = itemType.GetValueType(typeof(InputNormalization<>));
            if (inputValueType == null)
                return "None";

            var categoryAttribute = itemType.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute != null)
                return $"{categoryAttribute.Category}/{itemType.Name}";

            string inputValueTypeName = inputValueType.GetName();

            return $"{inputValueTypeName}/{itemType.Name}";
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();
            int result;
            if (categoryAttribute1 == null && categoryAttribute2 == null)
            {
                Type inputValueType1 = choice1.GetValueType(typeof(InputNormalization<>));
                if (inputValueType1 == null)
                    return -1;

                Type inputValueType2 = choice2.GetValueType(typeof(InputNormalization<>));
                if (inputValueType2 == null)
                    return 1;

                string inputValueTypeName1 = inputValueType1.GetName();
                string inputValueTypeName2 = inputValueType2.GetName();

                result = string.CompareOrdinal(inputValueTypeName1, inputValueTypeName2);
                if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);

                return result;
            }

            if (categoryAttribute1 == null)
                return 1;

            if (categoryAttribute2 == null)
                return -1;

            result = string.CompareOrdinal(categoryAttribute1.Category, categoryAttribute2.Category);
            if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
            return result;
        }
    }
}