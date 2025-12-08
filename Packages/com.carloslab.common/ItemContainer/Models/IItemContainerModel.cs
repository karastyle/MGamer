// <copyright file="IItemContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;

namespace CarlosLab.Common
{
    public interface IItemContainerModel<TItemModel> : IModel, IItemContainer
        where TItemModel : class, IModelWithId, IContainerItem
    {
        IReadOnlyList<TItemModel> Items { get; }
        bool TryAddItem(string name, TItemModel item);
        bool TryAddItemWithoutRuntime(string name, TItemModel item);

        bool TryAddItem(int index, string name, TItemModel item);

        bool TryRemoveItem(string name);
        bool TryRemoveItem(string name, out TItemModel item);
        bool TryRemoveItemWithoutRuntime(string name, out TItemModel item);

        bool TryGetItem(string name, out TItemModel item);
        TItemModel GetItemByName(string name);
        TItemModel GetItemById(string id);

        void MoveItem(int sourceIndex, int destIndex);
    }
}