// <copyright file="ItemContainerConverter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public class ItemContainerConverter<TContainer, TItem> : JsonConverter
        where TContainer : class, IItemContainerModel<TItem>, new()
        where TItem : class, IModelWithId, IContainerItem

    {
        public override bool CanConvert(Type objectType)
        {
            bool canConvert = typeof(IItemContainerModel<TItem>).IsAssignableFrom(objectType);
            return canConvert;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            List<TItem> list = serializer.Deserialize<List<TItem>>(reader);
            TContainer container = new();
            foreach (TItem item in list)
            {
                container.TryAddItemWithoutRuntime(item.Name, item);
            }

            return container;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value is IItemContainerModel<TItem> container)
            {
                var items = container.Items;
                for (int index = 0; index < items.Count; index++)
                {
                    TItem item = items[index];
                    serializer.Serialize(writer, item);
                }
            }

            writer.WriteEndArray();
        }
    }
}