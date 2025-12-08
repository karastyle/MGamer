// <copyright file="GenericModelConverter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using CarlosLab.Common.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CarlosLab.Common
{
    public class GenericModelConverter<TModel> : ModelConverter<TModel>
        where TModel : GenericModel, new()
    {
        private const string TypePropertyName = "$type";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TModel model = value as TModel;
            if (model == null)
                throw new JsonSerializationException($"Cannot serialize {value?.GetType().Name} as {nameof(GenericModel)}");

            OnSerializing(model, serializer.Context);

            writer.WriteStartObject();
            writer.WritePropertyName(TypePropertyName);
            serializer.Serialize(writer, model.RuntimeType.FullName);

            foreach (KeyValuePair<string, object> field in model.FieldValues)
            {
                writer.WritePropertyName(field.Key);

                object fieldValue;
                if (field.Value is LayerMask layerMask)
                    fieldValue = (int)layerMask;
                else
                    fieldValue = field.Value;

                serializer.Serialize(writer, fieldValue);
            }

            writer.WriteEndObject();

            OnSerialized(model, serializer.Context);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return null;

            TModel model = new();

            OnDeserializing(model, serializer.Context);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value!.ToString();
                    if (propertyName == TypePropertyName)
                    {
                        string typeName = reader.ReadAsString();
                        Type runtimeType = TypeUtils.GetType(typeName);
#if UNITY_EDITOR
                        if (runtimeType == null)
                        {
                            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<ClassFormerlySerializedAsAttribute>();
                            foreach (var type in types)
                            {
                                var attribute = type.GetCustomAttribute<ClassFormerlySerializedAsAttribute>();
                                string oldTypeName = string.IsNullOrEmpty(attribute.OldClassName) == false
                                    ? attribute.OldClassName
                                    : type.Name;
                                string oldNamespace = string.IsNullOrEmpty(attribute.OldNamespace) == false
                                    ? attribute.OldNamespace
                                    : type.Namespace;
                                if (!string.IsNullOrEmpty(oldNamespace))
                                    oldTypeName = $"{oldNamespace}.{oldTypeName}";

                                if (oldTypeName == typeName)
                                    runtimeType = type;
                            }
                        }
#endif                        
                        if (!model.Init(runtimeType))
                            throw new JsonSerializationException($"Cannot deserialize {typeName}");
                    }
                    else
                    {
                        reader.Read();

                        FieldInfo fieldInfo = model.FieldInfos.GetValueOrDefault(propertyName);

#if UNITY_EDITOR
                        if (fieldInfo == null)
                        {
                            var fields = TypeCache.GetFieldsWithAttribute<FieldFormerlySerializedAsAttribute>();
                            foreach (var field in fields)
                            {
                                var attribute = field.GetCustomAttribute<FieldFormerlySerializedAsAttribute>();
                                string oldFieldName = attribute.OldFieldName;
                                if (oldFieldName == propertyName && field.DeclaringType == model.RuntimeType)
                                {
                                    propertyName = field.Name;
                                    fieldInfo = field;
                                    break;
                                }
                            }
                        }
#endif

                        if (fieldInfo != null)
                        {
                            // Debug.Log($"ReadJson3 Type: {model.RuntimeType.Name} Property: {propertyName}");

                            object value;
                            if (fieldInfo.FieldType == typeof(LayerMask))
                            {
                                int layerMaskInt = (int)serializer.Deserialize(reader, typeof(int))!;
                                value = (LayerMask)layerMaskInt;
                            }
                            else
                            {
                                JsonToken tokenType = reader.TokenType;
                                int depth = reader.Depth;
                                try
                                {
                                    value = serializer.Deserialize(reader, fieldInfo.FieldType);
                                }
                                catch (JsonException)
                                {
                                    value = fieldInfo.FieldType.GetDefaultValue();
                                    reader.Skip(tokenType, depth);
                                }
                            }

                            model.SetValueWithoutRuntime(propertyName, value);
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
                else
                {
                    break;
                }

            }

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonSerializationException($"Unexpected Token: {reader.TokenType}");
            }

            OnDeserialized(model, serializer.Context);

            return model;
        }

        public override bool CanConvert(Type objectType)
        {
            bool canConvert = typeof(TModel).IsAssignableFrom(objectType);
            return canConvert;
        }
    }
}