// <copyright file="ItemContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Extensions;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public abstract class ItemContainerModel<TItemModel> : Model, IItemContainerModel<TItemModel>
        where TItemModel : class, IModelWithId, IContainerItem
    {
        private Dictionary<string, TItemModel> itemDict;
        protected readonly List<TItemModel> items = new();

        private Dictionary<string, TItemModel> ItemDict
        {
            get
            {
                if (itemDict == null)
                {
                    itemDict = new Dictionary<string, TItemModel>();

                    foreach (TItemModel item in items)
                    {
                        itemDict.TryAdd(item.Name, item);
                    }
                }

                return itemDict;
            }
        }

        public int Count => items.Count;

        public IReadOnlyList<TItemModel> Items => items;

        public bool Contains(string name)
        {
            return ItemDict.ContainsKey(name);
        }

        public bool TryAddItem(string name, TItemModel item)
        {
            return TryAddItem(Count, name, item);
        }

        public virtual bool TryAddItem(int index, string name, TItemModel item)
        {
            return TryAddItemWithoutRuntime(index, name, item);
        }

        public bool TryAddItemWithoutRuntime(string name, TItemModel item)
        {
            return TryAddItemWithoutRuntime(Count, name, item);
        }

        public bool TryRemoveItem(string name)
        {
            return TryRemoveItem(name, out _);
        }

        public virtual bool TryRemoveItem(string name, out TItemModel item)
        {
            return TryRemoveItemWithoutRuntime(name, out item);
        }

        public bool TryRemoveItemWithoutRuntime(string name, out TItemModel item)
        {
            item = null;
            if (string.IsNullOrEmpty(name))
                return false;

            if (ItemDict.Remove(name, out item))
            {
                items.Remove(item);
                item.HandleItemRemoved();
                return true;
            }

            return false;
        }

        public bool TryGetItem(string name, out TItemModel item)
        {
            item = null;
            if (string.IsNullOrEmpty(name))
                return false;

            return ItemDict.TryGetValue(name, out item);
        }

        public TItemModel GetItemByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return ItemDict.GetValueOrDefault(name);
        }

        public TItemModel GetItemById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return items.Find(x => x.Id == id);
        }

        public void MoveItem(int sourceIndex, int destIndex)
        {
            items.Move(sourceIndex, destIndex);
        }

        public bool TryAddItemWithoutRuntime(int index, string name, TItemModel item)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (ItemDict.TryAdd(name, item))
            {
                items.Insert(index, item);
                item.HandleItemAdded(this, name);
                return true;
            }

            return false;
        }
    }

    public class ItemContainerModel<TItemRuntime, TContainerRuntime> : ItemContainerModel<TItemRuntime>,
        IModel<TContainerRuntime>
        where TContainerRuntime : ItemContainer<TItemRuntime>, IRuntimeObject, new()
        where TItemRuntime : class, IRuntimeObject, IModelWithId, IContainerItem
    {
        private TContainerRuntime runtime;

        public TContainerRuntime Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = new TContainerRuntime();
                    foreach (TItemRuntime item in items)
                    {
                        runtime.TryAddItem(item.Name, item);
                    }
                }

                return runtime;
            }
        }

        public override bool TryAddItem(int index, string name, TItemRuntime item)
        {
            bool result = true;
            if (runtime != null)
                result = runtime.TryAddItem(name, item);

            if (result)
                return base.TryAddItem(index, name, item);

            return false;
        }

        public override bool TryRemoveItem(string name, out TItemRuntime item)
        {
            item = null;

            bool result = true;
            if (runtime != null)
                result = runtime.TryRemoveItem(name);

            if (result)
                return base.TryRemoveItem(name, out item);

            return false;
        }
    }

    public class ItemContainerModel<TItemModel, TItemRuntime, TContainerRuntime> : ItemContainerModel<TItemModel>,
        IModel<TContainerRuntime>
        where TContainerRuntime : ItemContainer<TItemRuntime>, IRuntimeObject, new()
        where TItemModel : class, IModel<TItemRuntime>, IModelWithId, IContainerItem
        where TItemRuntime : class, IRuntimeObject, IContainerItem
    {
        private TContainerRuntime runtime;

        public TContainerRuntime Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = new TContainerRuntime();
                    foreach (TItemModel item in items)
                    {
                        runtime.TryAddItem(item.Name, item.Runtime);
                    }
                }

                return runtime;
            }
        }

        public override bool TryAddItem(int index, string name, TItemModel item)
        {
            bool result = true;
            if (runtime != null)
                result = runtime.TryAddItem(name, item.Runtime);

            if (result)
                return base.TryAddItem(index, name, item);

            return false;
        }

        public override bool TryRemoveItem(string name, out TItemModel item)
        {
            item = null;

            bool result = true;
            if (runtime != null)
                result = runtime.TryRemoveItem(name);

            if (result)
                return base.TryRemoveItem(name, out item);

            return false;
        }
    }
}