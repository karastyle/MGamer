// <copyright file="FieldFormerlySerializedAsAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldFormerlySerializedAsAttribute : Attribute
    {
        public string OldFieldName;

        public FieldFormerlySerializedAsAttribute(string oldFieldName)
        {
            OldFieldName = oldFieldName;
        }
    }
}