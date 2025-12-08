// <copyright file="GenericModel_Runtime.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public abstract class GenericModel<TRuntime> : GenericModel, IModel<TRuntime>
        where TRuntime : class, IRuntimeObject
    {
        private TRuntime runtime;

        public override object RuntimeObject => Runtime;

        public TRuntime Runtime
        {
            get
            {
                if (runtime == null)
                    runtime = CreateRuntimeObject();
                return runtime;
            }
        }

        public override Type RuntimeBaseType => typeof(TRuntime);

        public override void SetValueRuntime(string fieldName, object value)
        {
            if (Initialized == false)
                return;

            SetValue(fieldName, value, runtime);
        }

        public override object GetValueRuntime(string fieldName)
        {
            if (Initialized == false)
                return null;

            return GetValue(fieldName, runtime);
        }

        protected virtual TRuntime CreateRuntimeObject()
        {
            if (!Initialized)
                return null;

            object runtime = Activator.CreateInstance(RuntimeType);
            foreach (KeyValuePair<string, object> field in FieldValues)
            {
                SetValue(field.Key, field.Value, runtime);
            }

            return runtime as TRuntime;
        }
    }
}