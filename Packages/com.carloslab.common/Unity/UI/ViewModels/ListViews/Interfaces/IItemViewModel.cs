// <copyright file="IItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public interface IItemViewModel : IViewModel
    {
        int Index { get; }
        bool IsSelected { get; }
        bool IsInList { get; }
        internal void HandleItemAdded(IListViewModel listViewModel);
        internal void HandleItemRemoved();
        void RemoveFromList();
        void RenameItem(string newName);
    }

    public interface IItemViewModelWithModel<TItemModel> : IItemViewModel, IViewModel<TItemModel>
        where TItemModel : class, IModelWithId
    {

    }

    public interface IItemViewModel<TItemModel, TListViewModel> : IItemViewModelWithModel<TItemModel>,
        IItemViewModelWithListViewModel<TListViewModel>
        where TItemModel : class, IModelWithId
        where TListViewModel : class, IListViewModel
    {

    }

    public interface IItemViewModelWithListViewModel<TListViewModel> : IItemViewModel
        where TListViewModel : class, IListViewModel
    {
        TListViewModel ListViewModel { get; internal set; }
    }
}