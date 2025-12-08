// <copyright file="CategoryAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CategoryAttribute : Attribute
    {
        public string Category;

        public CategoryAttribute(string category)
        {
            Category = category;
        }
    }
}