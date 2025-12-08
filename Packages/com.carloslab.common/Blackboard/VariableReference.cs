// <copyright file="VariableReference.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    [DataContract]
    public struct VariableReference<TValue> : IModel, IVariableReference
    {
        #region Fields

        [DataMember(Name = nameof(Name))]
        private string name;

        [DataMember(Name = nameof(ReferenceType))]
        private VariableReferenceType referenceType;

        [DataMember(Name = nameof(Value))]
        private TValue value;

        private Blackboard blackboard;
        private Variable<TValue> variable;

        #endregion

        #region VariableReference Properties

        public string Name => name;

        public VariableReferenceType ReferenceType
        {
            get => referenceType;
            private set
            {
                if (referenceType == value) return;
                referenceType = value;

                OnReferenceTypeChanged();
            }
        }

        public bool IsBlackboardReference => referenceType == VariableReferenceType.Blackboard;

        public object ValueObject
        {
            get => value;
            set
            {
                if (value is TValue tValue)
                    this.value = tValue;
            }
        }

        public TValue Value
        {
            get
            {
                if (Variable == null)
                    return value;

                return Variable.Value;
            }
            set
            {
                if (Variable == null)
                {
                    this.value = value;
                    return;
                }

                Variable.Value = value;
            }
        }

        public Variable<TValue> Variable
        {
            get
            {
                if (variable == null)
                    UpdateVariable();

                return variable;
            }
        }

        public Type ValueType => typeof(TValue);

        public Blackboard Blackboard => blackboard;

        #endregion

        #region IVariableReference Interface Properties

        string IVariableReference.Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                OnNameChanged();
            }
        }

        bool IVariableReference.IsBlackboardReference
        {
            get => referenceType == VariableReferenceType.Blackboard;
            set => ReferenceType = value ? VariableReferenceType.Blackboard : VariableReferenceType.None;
        }

        Blackboard IVariableReference.Blackboard
        {
            get => blackboard;
            set
            {
                if (blackboard == value) return;

                blackboard = value;
                OnBlackboardChanged();
            }
        }

        #endregion

        #region Operators

        public static implicit operator VariableReference<TValue>(TValue value)
        {
            return new VariableReference<TValue> { value = value };
        }

        public static implicit operator TValue(VariableReference<TValue> variableReference)
        {
            return variableReference.Value;
        }

        #endregion

        #region Functions

        private Variable<TValue> GetVariable(VariableReferenceType referenceType, string name)
        {
            if (referenceType == VariableReferenceType.Blackboard)
                return GetBlackboardVariable(name);

            return null;
        }

        private Variable<TValue> GetBlackboardVariable(string name)
        {
            if (string.IsNullOrEmpty(name) || blackboard == null)
                return null;

            if (blackboard.TryGetVariable<Variable<TValue>>(name, out var variable))
                return variable;
            else
                StaticConsole.LogWarning($"Asset: {Asset?.Name} Cannot find the Blackboard Variable: {name}." +
                                 $" Please remove it from your JSON using File Menu Toolbar.");

            return null;
        }

        public object Clone()
        {
            return this;
        }

        private void UpdateVariable()
        {
            variable = GetVariable(referenceType, name);
        }

        #endregion

        #region Event Functions

        private void OnNameChanged()
        {
            UpdateVariable();
        }

        private void OnBlackboardChanged()
        {
            UpdateVariable();
        }

        private void OnReferenceTypeChanged()
        {
            // CommonConsole.Instance.Log($"IsBlackboardReference OnReferenceTypeChanged: {referenceType}");
            UpdateVariable();
        }

        #endregion

        public IDataAsset Asset { get; private set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is IDataAsset asset)
            {
                Asset = asset;
            }
        }
    }
}