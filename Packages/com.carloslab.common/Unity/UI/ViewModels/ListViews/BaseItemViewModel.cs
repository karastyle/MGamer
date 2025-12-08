// <copyright file="BaseItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public abstract class BaseItemViewModel<TItemModel, TListViewModel, TRootViewModel>
        : RootViewModelMember<TItemModel, TRootViewModel>
            , IItemViewModel<TItemModel, TListViewModel>
        where TItemModel : class, IModelWithId
        where TListViewModel : class, IListViewModel
        where TRootViewModel : class, IRootViewModel
    {
        protected sealed override void HandleRootViewModelChanged(TRootViewModel rootViewModel)
        {
            base.HandleRootViewModelChanged(rootViewModel);
        }

        #region ListViewModel

        private TListViewModel listViewModel;

        TListViewModel IItemViewModelWithListViewModel<TListViewModel>.ListViewModel
        {
            get => listViewModel;
            set => listViewModel = value;
        }

        public TListViewModel ListViewModel => listViewModel;

        #endregion

        #region IItemViewModel

        public int Index => listViewModel?.GetItemIndex(this) ?? -1;

        public bool IsSelected => Index == listViewModel.SelectedIndex;

        private bool isInList;
        public bool IsInList => isInList;
        void IItemViewModel.HandleItemAdded(IListViewModel listViewModel)
        {
            this.listViewModel = listViewModel as TListViewModel;

            if (this.listViewModel == null) return;

            this.isInList = true;

            OnItemAdded();
        }

        void IItemViewModel.HandleItemRemoved()
        {
            this.listViewModel = null;
            this.isInList = false;

            OnItemRemoved();
        }

        public void RemoveFromList()
        {
            listViewModel?.RemoveItemAt(Index);
        }

        public void RenameItem(string newName)
        {
            listViewModel?.TryRenameItem(this, newName);
        }

        protected virtual void OnItemAdded()
        {

        }

        protected virtual void OnItemRemoved()
        {

        }

        #endregion

    }
}