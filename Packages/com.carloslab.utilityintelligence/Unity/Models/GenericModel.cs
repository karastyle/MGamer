// <copyright file="GenericModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System.Collections.Generic;
using System.Reflection;

namespace CarlosLab.UtilityIntelligence
{
    public class GenericModel<TRuntime> : CarlosLab.Common.GenericModel<TRuntime>, IGenericModel
        where TRuntime : class, IRuntimeObject
    {
        private readonly List<FieldInfo> variableReferenceFields = new();

        protected override void OnFieldAdded(FieldInfo fieldInfo)
        {
            if (typeof(IVariableReference).IsAssignableFrom(fieldInfo.FieldType))
                variableReferenceFields.Add(fieldInfo);
        }

        public void SetVariableReference(string oldVariableName, string newVariableName)
        {
            foreach (FieldInfo variableReferenceField in variableReferenceFields)
            {
                var fieldValue = GetValue(variableReferenceField.Name);
                if (fieldValue is IVariableReference variableReference
                    && variableReference.Name == oldVariableName)
                {
                    variableReference.Name = newVariableName; //Only Model
                    SetValueRuntime(variableReferenceField.Name, variableReference);
                }
            }
        }

        public void SetVariableReference(Blackboard blackboard)
        {
            foreach (FieldInfo variableReferenceField in variableReferenceFields)
            {
                var fieldValue = GetValue(variableReferenceField.Name);
                if (fieldValue is IVariableReference variableReference)
                {
                    variableReference.Blackboard = blackboard; //Only Model
                }
            }
        }
    }
}