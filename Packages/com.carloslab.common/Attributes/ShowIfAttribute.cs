// <copyright file="ShowIfAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowIfAttribute : Attribute
    {
        public string FieldName;

        public object FieldValue;

        public ShowIfAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public ShowIfAttribute(string fieldName, object fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }
}