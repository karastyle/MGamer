// <copyright file="ItemContainer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public abstract class ItemContainer<TItem> : IItemContainer
        where TItem : class, IContainerItem
    {
        protected readonly Dictionary<string, TItem> itemDict = new();
        protected readonly List<TItem> items = new();
        public IReadOnlyList<TItem> Items => items;

        public int Count => items.Count;

        public bool Contains(string name)
        {
            return itemDict.ContainsKey(name);
        }

        public event Action<TItem> ItemAdded;
        public event Action<TItem> ItemRemoved;

        internal bool TryAddItem(string name, TItem item)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (itemDict.TryAdd(name, item))
            {
                items.Add(item);

                item.HandleItemAdded(this, name);
                OnItemAdded(item);
                ItemAdded?.Invoke(item);
                return true;
            }

            return false;
        }

        protected virtual void OnItemAdded(TItem item)
        {

        }

        internal bool TryRemoveItem(string name)
        {
            return TryRemoveItem(name, out _);
        }

        internal bool TryRemoveItem(string name, out TItem item)
        {
            item = null;
            if (string.IsNullOrEmpty(name))
                return false;

            if (itemDict.Remove(name, out item))
            {
                items.Remove(item);

                item.HandleItemRemoved();
                OnItemRemoved(item);
                ItemRemoved?.Invoke(item);
                return true;
            }

            return false;
        }

        protected virtual void OnItemRemoved(TItem item)
        {

        }

        internal bool TryGetItem(string name, out TItem item)
        {
            item = null;
            if (string.IsNullOrEmpty(name))
                return false;

            return itemDict.TryGetValue(name, out item);
        }

        internal TItem GetItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return itemDict.GetValueOrDefault(name);
        }
    }

    public abstract class ItemValueContainer<TItem> : ItemContainer<TItem>
        where TItem : class, IContainerItemValue
    {
        public TValue GetValue<TValue>(string name)
        {
            IContainerItemValue<TValue> item = GetItemByValue<TValue>(name);
            if (item != null)
                return item.Value;

            StaticConsole.LogWarning($"Cannot find the variable: {name}");
            return default;
        }

        internal IContainerItemValue<TValue> GetItemByValue<TValue>(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (itemDict.TryGetValue(name, out TItem item))
                return item as IContainerItemValue<TValue>;

            StaticConsole.LogWarning($"Cannot find the variable: {name}");
            return null;
        }

        internal TItemValue GetItem<TItemValue>(string name)
            where TItemValue : class, IContainerItemValue
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (itemDict.TryGetValue(name, out TItem item))
                return item as TItemValue;

            StaticConsole.LogWarning($"Cannot find the variable: {name}");
            return null;
        }

        internal bool TryGetItem<TItemValue>(string name, out TItemValue itemValue)
            where TItemValue : class, IContainerItemValue
        {
            itemValue = null;
            if (string.IsNullOrEmpty(name))
                return false;

            itemDict.TryGetValue(name, out TItem item);
            {
                itemValue = item as TItemValue;
                if (itemValue != null)
                    return true;
            }
            return false;
        }
    }
}