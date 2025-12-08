// <copyright file="FoldoutGroupAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutGroupAttribute : Attribute
    {
        public string GroupName;

        public FoldoutGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}