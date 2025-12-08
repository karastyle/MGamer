// <copyright file="IListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.Common.UI
{
    public interface IListViewModel : IViewModel
    {
        int SelectedIndex { get; set; }

        void NotifyItemIndexChanged(int sourceIndex, int destIndex);

        void RemoveItemAt(int index);
        int GetItemIndex(IItemViewModel item);

        bool TryRenameItem(IItemViewModel item, string newName);
    }

    public interface IListViewModel<TItemModel, TItemViewModel> : IListViewModelWithViewModel<TItemViewModel>,
        IListViewModelWithModel<TItemModel>
        where TItemModel : class, IModelWithId
        where TItemViewModel : class, IItemViewModel
    {
    }

    public interface IListViewModelWithModel<TItemModel> : IListViewModel where TItemModel : class
    {
        IReadOnlyList<TItemModel> ItemModels { get; }
    }

    public interface IListViewModelWithViewModel<TItemViewModel> : IListViewModel
        where TItemViewModel : IItemViewModel
    {
        IReadOnlyList<TItemViewModel> Items { get; }

        TItemViewModel SelectedItem { get; }

        event Action<TItemViewModel> ItemAdded;
        event Action<TItemViewModel> ItemRemoved;
        event Action<TItemViewModel> ItemRemoving;

        TItemViewModel CreateItem(Type modelType = null, object itemInfo = null);

        bool TryAddItem(TItemViewModel item);
        bool TryRemoveItem(TItemViewModel item);
        bool TryRenameItem(TItemViewModel item, string newName);
    }
}