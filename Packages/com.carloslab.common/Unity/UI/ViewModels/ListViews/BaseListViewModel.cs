// <copyright file="BaseListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;

namespace CarlosLab.Common.UI
{
    public abstract class BaseListViewModel<TModel, TItemModel, TItemViewModel, TRootViewModel> : RootViewModelMember<TModel, TRootViewModel>, IListViewModel<TItemModel, TItemViewModel>
        where TModel : class, IModel
        where TItemModel : class, IModelWithId
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, IRootViewModelMember<TRootViewModel>, new()
        where TRootViewModel : class, IRootViewModel
    {
        #region BaseListViewModel

        protected override void OnInit(TModel model)
        {
            CreateItems();
        }

        private void CreateItems()
        {
            var models = ItemModels;
            for (int index = 0; index < models.Count; index++)
            {
                TItemModel itemModel = models[index];
                TryCreateItemWithoutModel(itemModel, out _);
            }
        }

        protected override void OnDeinit()
        {
            ClearItems();
        }

        private void ClearItems()
        {
            var items = this.items.ToArray();
            for (int index = 0; index < items.Length; index++)
            {
                var item = items[index];
                TryRemoveItemWithoutModel(item);
            }
        }

        protected sealed override void HandleRootViewModelChanged(TRootViewModel rootViewModel)
        {
            foreach (var item in items)
            {
                item.RootViewModel = rootViewModel;
            }
            base.HandleRootViewModelChanged(rootViewModel);
        }

        #endregion

        #region Model

        protected sealed override void OnModelChanged(TModel newModel)
        {
            //Undo/Redo
            if (newModel != null)
            {
                UpdateItemModels();
            }
            else
            {
                ResetItemModels();
            }
        }


        private void UpdateItemModels()
        {
            var models = ItemModels;
            if (models.Count >= items.Count)
            {
                for (int index = 0; index < models.Count; index++)
                {
                    TItemModel model = models[index];
                    TItemViewModel item = GetItemByModelId(model.Id);
                    if (item != null)
                        item.Model = model;
                    else
                        TryCreateItemWithoutModel(model, index, out _, true);
                }
            }
            else
            {
                var items = this.items.ToArray();

                for (int index = 0; index < items.Length; index++)
                {
                    TItemViewModel item = items[index];
                    TItemModel model = GetModelById(item.Model.Id);
                    if (model != null)
                        item.Model = model;
                    else
                        TryRemoveItemWithoutModel(item, true);
                }
            }
        }


        private void ResetItemModels()
        {
            foreach (var item in items)
            {
                item.Model = null;
            }
        }


        #endregion

        #region ItemModel

        public abstract IReadOnlyList<TItemModel> ItemModels { get; }

        protected virtual TItemModel CreateModel(Type runtimeType)
        {
            return ModelFactory<TItemModel>.Create(runtimeType);
        }

        public TItemModel GetModelById(string id)
        {
            return ItemModels.FirstOrDefault(x => x.Id == id);
        }

        #endregion

        #region Add ItemModel

        private bool TryAddModel(TItemModel model)
        {
            bool result = false;
            Record($"Add Item: {typeof(TItemModel).Name}",
                () => { result = TryAddModelWithoutRecord(model); });

            return result;
        }

        private bool TryAddModelWithoutRecord(TItemModel model)
        {
            return TryAddModelWithoutRecord(model, ItemModels.Count);
        }

        protected abstract bool TryAddModelWithoutRecord(TItemModel model, int index);


        #endregion

        #region Remove ItemModel

        private bool TryRemoveModel(TItemModel item)
        {
            bool result = false;

            Record($"Remove Item: {typeof(TItemModel).Name}",
                () => { result = TryRemoveModelWithoutRecord(item); });

            return result;
        }

        protected abstract bool TryRemoveModelWithoutRecord(TItemModel model);


        #endregion

        #region ItemViewModel

        protected readonly List<TItemViewModel> items = new();

        private readonly Dictionary<string, TItemViewModel> itemDict = new();

        [CreateProperty] public IReadOnlyList<TItemViewModel> Items => items;

        public TItemViewModel GetItemByModelId(string modelId)
        {
            return items.Find(x => x.Model.Id == modelId);
        }

        public bool TryRenameItem(IItemViewModel item, string newName)
        {
            var itemViewModel = item as TItemViewModel;
            if (itemViewModel == null) return false;

            return TryRenameItem(itemViewModel, newName);

        }

        public abstract bool TryRenameItem(TItemViewModel item, string newName);

        #endregion

        #region SelectedItem

        private int selectedIndex = -1;

        [CreateProperty]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value)
                    return;

                selectedIndex = value;
            }
        }

        public TItemViewModel SelectedItem
        {
            get
            {
                if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                    return null;

                return Items[SelectedIndex];
            }
        }

        #endregion

        #region ItemIndex

        public event Action<int, int> ItemIndexChanged;

        protected void RaiseItemIndexChanged(int sourceIndex, int destIndex)
        {
            ItemIndexChanged?.Invoke(sourceIndex, destIndex);
        }

        public int GetItemIndex(IItemViewModel item)
        {
            TItemViewModel itemViewModel = item as TItemViewModel;
            if (itemViewModel == null)
                return -1;

            return items.IndexOf(itemViewModel);
        }

        protected void HandleItemIndexChanged(int sourceIndex, int destIndex)
        {
            items.Move(sourceIndex, destIndex);
            NotifyItemIndexChanged(sourceIndex, destIndex);
        }

        public void NotifyItemIndexChanged(int sourceIndex, int destIndex)
        {
            RaiseItemIndexChanged(sourceIndex, destIndex);
            OnItemIndexChanged(sourceIndex, destIndex);
        }

        protected virtual void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
        }

        #endregion

        #region Create ItemViewModel

        public virtual TItemViewModel CreateItem(Type modelType, object itemInfo)
        {
            TItemModel model = CreateModel(modelType);
            TryCreateItem(model, out TItemViewModel item);
            return item;
        }

        public bool TryCreateItem(TItemModel model, out TItemViewModel newItem)
        {
            newItem = null;

            TItemViewModel viewModel = CreateItem(model);
            if (TryAddItem(viewModel))
            {
                newItem = viewModel;
                return true;
            }

            return false;
        }

        protected bool TryCreateItemWithoutModel(TItemModel model, out TItemViewModel item)
        {
            return TryCreateItemWithoutModel(model, items.Count, out item);
        }

        protected bool TryCreateItemWithoutModel(TItemModel model, int index, out TItemViewModel newItem,
            bool notify = false)
        {
            newItem = null;

            TItemViewModel viewModel = CreateItem(model);
            if (TryAddItemWithoutModel(viewModel, index, notify))
            {
                newItem = viewModel;
                return true;
            }

            return false;
        }

        protected virtual TItemViewModel CreateItem(TItemModel model)
        {
            TItemViewModel item = new();
            item.RootViewModel = RootViewModel;
            item.Init(model);
            return item;
        }

        #endregion

        #region Add ItemViewModel

        public event Action<TItemViewModel> ItemAdded;

        protected void RaiseItemAdded(TItemViewModel item)
        {
            ItemAdded?.Invoke(item);
        }

        public bool TryAddItem(TItemViewModel item)
        {
            if (item == null)
                return false;

            if (TryAddModel(item.Model) && TryAddItemWithoutModel(item, true))
                return true;

            return false;
        }

        protected bool TryAddItemWithoutModel(TItemViewModel item, bool notify = false)
        {
            return TryAddItemWithoutModel(item, items.Count, notify);
        }

        protected bool TryAddItemWithoutModel(TItemViewModel item, int index, bool notify = false)
        {
            if (item == null)
                return false;

            if (itemDict.TryAdd(item.Model.Id, item))
            {
                items.Insert(index, item);
                HandleItemAdded(item);

                if (notify)
                    RaiseItemAdded(item);
                return true;
            }

            return false;
        }

        private void HandleItemAdded(TItemViewModel item)
        {
            item.RootViewModel = RootViewModel;
            item.HandleItemAdded(this);
            OnItemAdded(item);
        }

        protected virtual void OnItemAdded(TItemViewModel item)
        {
        }

        #endregion

        #region Remove ItemViewModel

        public event Action<TItemViewModel> ItemRemoved;
        public event Action<TItemViewModel> ItemRemoving;

        protected void RaiseItemRemoved(TItemViewModel item)
        {
            ItemRemoved?.Invoke(item);
        }

        protected void RaiseItemRemoving(TItemViewModel item)
        {
            ItemRemoving?.Invoke(item);
        }

        public void RemoveItemAt(int index)
        {
            TryRemoveItem(items[index]);
        }

        public bool TryRemoveItem(TItemViewModel item)
        {
            if (item == null)
                return false;

            if (TryRemoveModel(item.Model) && TryRemoveItemWithoutModel(item, true))
                return true;
            return false;
        }

        protected bool TryRemoveItemWithoutModelById(string id, bool notify = false)
        {
            var item = GetItemByModelId(id);
            return TryRemoveItemWithoutModel(item, notify);
        }

        protected bool TryRemoveItemWithoutModel(TItemViewModel item, bool notify = false)
        {
            ;
            if (item == null)
                return false;

            if (itemDict.Remove(item.Model.Id))
            {
                if (notify)
                    RaiseItemRemoving(item);

                items.Remove(item);

                if (notify)
                    RaiseItemRemoved(item);

                HandleItemRemoved(item);

                OnItemRemoved(item);

                return true;
            }

            return false;
        }

        private void HandleItemRemoved(TItemViewModel item)
        {
            item.HandleItemRemoved();

            item.RootViewModel = null;
            item.Deinit();
        }

        protected virtual void OnItemRemoved(TItemViewModel item)
        {
        }

        #endregion
    }
}