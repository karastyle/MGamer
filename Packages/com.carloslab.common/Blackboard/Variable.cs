// <copyright file="Variable.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    public abstract class Variable : ContainerItem<Blackboard>, IRuntimeObject, IContainerItemValue
    {
        [DataMember(Name = nameof(Category))]
        private string category;

        public string Category
        {
            get => category;
            internal set => category = value;
        }

        public abstract object ValueObject { get; set; }

        public abstract Type ValueType { get; }
        public Type VariableType => GetType();
    }

    public abstract class Variable<TValueType> : Variable, IContainerItemValue<TValueType>
    {
        [DataMember(Name = nameof(Value))]
        private TValueType value;

        #region Properties

        public TValueType Value
        {
            get => value;
            set => this.value = value;
        }

        public override object ValueObject
        {
            get => Value;
            set
            {
                if (value is TValueType tValue)
                    this.value = tValue;
            }
        }

        public override Type ValueType => typeof(TValueType);

        #endregion
    }
}