// <copyright file="BaseListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class BaseListView<TListViewModel, TItemViewModel, TListView, TRootView> :
        RootViewMember<TListViewModel, TRootView>
        , IListViewWithItem<TItemViewModel>
        where TListView : BaseListView, new()
        where TListViewModel : class, IRootViewModelMember, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel
        where TRootView : BaseView, IRootView
    {

        #region BaseListView

        protected BaseListView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        protected override void OnLoadVisualAssetFailed()
        {
            listView = new TListView();
            Add(listView);
            InitListView(listView);
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            listView = this.Q<TListView>();
            InitListView(listView);
        }

        protected virtual void InitListView(TListView listView)
        {
            SelectionType = SelectionType.Single;
            VirtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            // ReorderMode = ListViewReorderMode.Animated;
            Reorderable = false;

            RegisterListViewEvents(listView);
            OnInitListView(listView);
        }

        protected virtual void OnInitListView(TListView listView)
        {
        }

        protected override void OnRefreshView(TListViewModel viewModel)
        {
            listView.SetBinding("listview-custom-binding", new ListViewCustomBinding
            {
                dataSourcePath = PropertyPath.FromName(nameof(IListViewModelWithViewModel<TItemViewModel>.Items))
            });
        }

        protected override void OnResetView()
        {
            listView.ClearBindings();
        }

        protected sealed override void HandleRootViewChanged(TRootView rootView)
        {
            if (IsRuntimeUI)
                FixedItemHeight = 38;

            var items = Items;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item is IRootViewMember<TRootView> rootViewMember)
                        rootViewMember.RootView = RootView;
                }
            }

            base.HandleRootViewChanged(rootView);
        }

        #endregion

        #region ListView's Properties

        public IList Items => listView.itemsSource;

        public float FixedItemHeight
        {
            get => listView.fixedItemHeight;
            set => listView.fixedItemHeight = value;
        }

        public SelectionType SelectionType
        {
            get => listView.selectionType;
            set => listView.selectionType = value;
        }

        public CollectionVirtualizationMethod VirtualizationMethod
        {
            get => listView.virtualizationMethod;
            set => listView.virtualizationMethod = value;
        }

        public bool Reorderable
        {
            get => listView.reorderable;
            set
            {
                listView.reorderMode = value ? ListViewReorderMode.Animated : ListViewReorderMode.Simple;
                listView.reorderable = value;
            }
        }

        public ListViewReorderMode ReorderMode
        {
            get => listView.reorderMode;
            set => listView.reorderMode = value;
        }

        #endregion

        #region ListView

        protected TListView listView;

        private void RegisterListViewEvents(TListView listView)
        {
            listView.itemsAdded += ListView_OnItemsAdded;
            listView.itemsRemoved += ListView_OnItemsRemoved;
            listView.itemsSourceChanged += ListView_OnItemsSourceChanged;
            listView.selectionChanged += ListView_OnSelectionChanged;
            listView.itemIndexChanged += ListView_OnItemIndexChanged;
            listView.itemsChosen += ListView_OnItemChosen;
            listView.selectedIndicesChanged += ListView_OnSelectedIndicesChanged;
        }

        private void ListView_OnItemsRemoved(IEnumerable<int> obj)
        {
            CommonConsole.Instance.Log($"{GetType().Name} ListView_OnItemsRemoved");
        }

        private void ListView_OnItemsAdded(IEnumerable<int> obj)
        {
            CommonConsole.Instance.Log($"{GetType().Name} ListView_OnItemsAdded");
        }

        private void ListView_OnItemChosen(IEnumerable<object> items)
        {
            CommonConsole.Instance.Log($"{GetType().Name} ListView_OnItemChosen");
        }


        private void ListView_OnSelectedIndicesChanged(IEnumerable<int> items)
        {
            CommonConsole.Instance.Log($"{GetType().Name} ListView_OnSelectedIndicesChanged");
        }

        #endregion

        #region ItemSources

        public event Action<IList> ItemsSourceChanged;

        protected virtual void OnItemsSourceChanged()
        {
        }

        private void ListView_OnItemsSourceChanged()
        {
            CommonConsole.Instance.Log(
                $"{GetType().Name} RaiseItemsSourceChanged SelectedIndex: {SelectedIndex} ModelSelectedIndex: {ViewModel.SelectedIndex}");
            ItemsSourceChanged?.Invoke(Items);

            OnItemsSourceChanged();
        }

        #endregion

        #region ItemIndex

        private void ListView_OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            CommonConsole.Instance.Log($"{GetType().Name} HandleItemIndexChanged {sourceIndex} {destIndex}");

            ViewModel.NotifyItemIndexChanged(sourceIndex, destIndex);
            OnItemIndexChanged(sourceIndex, destIndex);
        }

        protected virtual void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
        }


        #endregion

        #region Selection

        public TItemViewModel SelectedItem => (TItemViewModel)listView.selectedItem;

        public int SelectedIndex
        {
            get => listView.selectedIndex;
            set
            {
                if (listView.selectedIndex == value)
                    return;

                listView.selectedIndex = value;
                OnSelectedIndexChanged(value);
            }
        }

        public void ClearSelection()
        {
            SelectedIndex = -10;
        }

        protected virtual void OnSelectedIndexChanged(int index)
        {
        }

        private void ListView_OnSelectionChanged(IEnumerable<object> items)
        {
            ViewModel.SelectedIndex = SelectedIndex;
            CommonConsole.Instance.Log($"{GetType().Name} HandleSelectionChanged SelectedIndex: {SelectedIndex}");

            OnSelectionChanged(items);
        }

        protected virtual void OnSelectionChanged(IEnumerable<object> items)
        {
        }


        #endregion

        #region ListViewModel

        protected override void OnRegisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded += HandleItemAdded;
            viewModel.ItemRemoving += HandleItemRemoving;
            viewModel.ItemRemoved += HandleItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded -= HandleItemAdded;
            viewModel.ItemRemoving -= HandleItemRemoving;
            viewModel.ItemRemoved -= HandleItemRemoved;
        }


        public bool TryRenameItem(TItemViewModel item, string newName)
        {
            return ViewModel.TryRenameItem(item, newName);
        }

        #endregion

        #region Add Item

        public event Action<TItemViewModel> ItemAdded;

        public bool TryAddItem(TItemViewModel item)
        {
            return ViewModel.TryAddItem(item);
        }

        private void HandleItemAdded(TItemViewModel newItem)
        {
            if (newItem.Index == SelectedIndex)
                SelectedIndex = SelectedItem.Index;

            OnItemAdded(newItem);

            ItemAdded?.Invoke(newItem);
        }

        protected virtual void OnItemAdded(TItemViewModel newItem)
        {
        }


        #endregion

        #region Remove Item
        public event Action<TItemViewModel> ItemRemoved;

        public bool TryRemoveItem(TItemViewModel item)
        {
            return ViewModel.TryRemoveItem(item);
        }

        private void HandleItemRemoved(TItemViewModel item)
        {
            ItemRemoved?.Invoke(item);
            OnItemRemoved(item);
        }

        protected virtual void OnItemRemoved(TItemViewModel item)
        {
        }

        private void HandleItemRemoving(TItemViewModel item)
        {
            if (item.Index == SelectedIndex)
                ClearSelection();
        }

        #endregion
    }
}