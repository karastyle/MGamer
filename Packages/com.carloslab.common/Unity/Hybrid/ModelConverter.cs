// <copyright file="ModelConverter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    public abstract class ModelConverter<TModel> : JsonConverter
        where TModel : IModel, new()
    {
        private List<MethodInfo> onSerializingCallbacks = new();
        private List<MethodInfo> onSerializedCallbacks = new();

        private List<MethodInfo> onDeserializingCallbacks = new();
        private List<MethodInfo> onDeserializedCallbacks = new();

        public ModelConverter()
        {
            InitCallbacks();
        }

        private void InitCallbacks()
        {
            var methods = typeof(TModel).GetMethods(
                BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.Public);

            MethodInfo currentOnSerializing = null;
            MethodInfo currentOnSerialized = null;
            MethodInfo currentOnDeserializing = null;
            MethodInfo currentOnDeserialized = null;

            foreach (var method in methods)
            {
                if (method.ContainsGenericParameters)
                {
                    continue;
                }

                Type prevAttributeType = null;
                ParameterInfo[] parameters = method.GetParameters();

                if (IsValidCallback(method, parameters, typeof(OnSerializingAttribute), currentOnSerializing,
                        ref prevAttributeType))
                {
                    onSerializingCallbacks.Add(method);
                    currentOnSerializing = method;
                }

                if (IsValidCallback(method, parameters, typeof(OnSerializedAttribute), currentOnSerialized,
                        ref prevAttributeType))
                {
                    onSerializedCallbacks.Add(method);
                    currentOnSerialized = method;
                }

                if (IsValidCallback(method, parameters, typeof(OnDeserializingAttribute), currentOnDeserializing,
                        ref prevAttributeType))
                {
                    onDeserializingCallbacks.Add(method);
                    currentOnDeserializing = method;
                }

                if (IsValidCallback(method, parameters, typeof(OnDeserializedAttribute), currentOnDeserialized,
                        ref prevAttributeType))
                {
                    onDeserializedCallbacks.Add(method);
                    currentOnDeserialized = method;
                }
            }
        }

        private bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType,
            MethodInfo currentCallback, ref Type prevAttributeType)
        {
            if (!method.IsDefined(attributeType, false))
            {
                return false;
            }

            if (currentCallback != null)
            {
                string message =
                    $"Invalid attribute. Both '{method}' and '{currentCallback}' in type '{method.DeclaringType?.FullName}' have '{attributeType}'.";
                throw new JsonException(message);
            }

            if (prevAttributeType != null)
            {
                string message =
                    $"Invalid Callback. Method '{method}' in type '{method.DeclaringType?.FullName}' has both '{prevAttributeType}' and '{attributeType}'.";
                throw new JsonException(message);
            }

            if (method.IsVirtual)
            {
                string message =
                    $"Virtual Method '{method}' of type '{method.DeclaringType?.FullName}' cannot be marked with '{attributeType}' attribute.";
                throw new JsonException(message);
            }

            if (method.ReturnType != typeof(void))
            {
                string message =
                    $"Serialization Callback '{method}' in type '{method.DeclaringType?.FullName}' must return void.";
                throw new JsonException(message);
            }

            if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(StreamingContext))
            {
                string message =
                    $"Serialization Callback '{method}' in type '{method.DeclaringType?.FullName}' must have a single parameter of type '{typeof(StreamingContext)}'.";
                throw new JsonException(message);
            }

            return true;
        }

        protected void OnSerializing(object model, StreamingContext context)
        {
            object[] parameters = { context };
            foreach (var callback in onSerializingCallbacks)
            {
                callback?.Invoke(model, parameters);
            }
        }

        protected void OnSerialized(object model, StreamingContext context)
        {
            object[] parameters = { context };
            foreach (var callback in onSerializedCallbacks)
            {
                callback?.Invoke(model, parameters);
            }
        }

        protected void OnDeserializing(object model, StreamingContext context)
        {
            object[] parameters = { context };
            foreach (var callback in onDeserializingCallbacks)
            {
                callback?.Invoke(model, parameters);
            }
        }

        protected void OnDeserialized(object model, StreamingContext context)
        {
            object[] parameters = { context };
            foreach (var callback in onDeserializedCallbacks)
            {
                callback?.Invoke(model, parameters);
            }
        }
    }
}