// <copyright file="ClassFormerlySerializedAsAttribute.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ClassFormerlySerializedAsAttribute : Attribute
    {
        public string OldClassName;
        public string OldNamespace;

        public ClassFormerlySerializedAsAttribute(string oldClassName = null, string oldNamespace = null)
        {
            OldClassName = oldClassName;
            OldNamespace = oldNamespace;
        }
    }
}