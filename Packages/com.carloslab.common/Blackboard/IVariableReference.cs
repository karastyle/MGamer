// <copyright file="IVariableReference.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public interface IVariableReference : ICloneable
    {
        string Name { get; internal set; }
        bool IsBlackboardReference { get; internal set; }

        VariableReferenceType ReferenceType { get; }
        object ValueObject { get; set; }
        Blackboard Blackboard { get; internal set; }
        Type ValueType { get; }
    }
}