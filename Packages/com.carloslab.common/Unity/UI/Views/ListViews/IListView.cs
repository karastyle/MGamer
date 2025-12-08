// <copyright file="IListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections;

namespace CarlosLab.Common.UI
{
    public interface IListView
    {
        int SelectedIndex { get; set; }
        IList Items { get; }
        bool Reorderable { get; set; }
    }

    public interface IListViewWithItem<TItemViewModel> : IListView
        where TItemViewModel : IItemViewModel
    {
        TItemViewModel SelectedItem { get; }
        event Action<TItemViewModel> ItemAdded;
        event Action<TItemViewModel> ItemRemoved;

        bool TryAddItem(TItemViewModel item);
        bool TryRenameItem(TItemViewModel item, string newName);
        bool TryRemoveItem(TItemViewModel item);
    }

    public interface IListViewWithList<TListViewModel> : IListView
        where TListViewModel : IListViewModel
    {
        TListViewModel ViewModel { get; }
    }
}