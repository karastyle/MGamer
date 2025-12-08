// <copyright file="HideIfAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HideIfAttribute : Attribute
    {
        public string FieldName;

        public object FieldValue;

        public HideIfAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public HideIfAttribute(string fieldName, object fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }
}