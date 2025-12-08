// <copyright file="DictionaryConverter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public class DictionaryConverter<TKey, TValue> : JsonConverter
    {
        private readonly Func<TValue, TKey> getKeyFunc;

        public DictionaryConverter(Func<TValue, TKey> getKeyFunc)
        {
            this.getKeyFunc = getKeyFunc;
        }

        public override bool CanConvert(Type objectType)
        {
            bool canConvert = typeof(Dictionary<TKey, TValue>).IsAssignableFrom(objectType);
            return canConvert;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value is Dictionary<TKey, TValue> dict)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in dict)
                {
                    serializer.Serialize(writer, kvp.Value);
                }
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // Debug.Log("ReadJson: " + objectType);
            List<TValue> list = serializer.Deserialize<List<TValue>>(reader);
            Dictionary<TKey, TValue> dict = new();
            foreach (TValue item in list)
            {
                dict[getKeyFunc(item)] = item;
            }

            return dict;
        }
    }
}