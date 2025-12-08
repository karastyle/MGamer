// <copyright file="BoxGroupAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BoxGroupAttribute : Attribute
    {
        public string GroupName;

        public BoxGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}