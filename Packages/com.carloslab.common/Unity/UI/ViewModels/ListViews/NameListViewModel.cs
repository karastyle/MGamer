// <copyright file="NameListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public abstract class NameListViewModel<Tmodel, TItemModel, TItemViewModel, TRootViewModel> :
        BaseListViewModel<Tmodel, TItemModel, TItemViewModel, TRootViewModel>, INameListViewModel
        where Tmodel : class, IModel
        where TItemModel : class, IModelWithId, IContainerItem
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, INameViewModel
        , IRootViewModelMember<TRootViewModel>, new()
        where TRootViewModel : class, IRootViewModel
    {
        public abstract bool Contains(string name);

        public override TItemViewModel CreateItem(Type modelType, object itemInfo)
        {
            string name = itemInfo as string;
            if (Contains(name))
                return null;

            TItemModel model = CreateModel(modelType);
            model.Name = name;

            TryCreateItem(model, out TItemViewModel item);
            return item;
        }

        public override bool TryRenameItem(TItemViewModel item, string newName)
        {
            if (!Contains(newName))
            {
                string oldName = item.Name;
                item.Name = newName;
                if (TryRemoveModelWithoutRecord(oldName, item.Model) &&
                    TryAddModelWithoutRecord(item.Model, item.Index)) return true;
            }

            return false;
        }

        public bool TryRemoveItem(string name)
        {
            TItemViewModel item = GetItemByName(name);
            return TryRemoveItem(item);
        }

        public bool TryRemoveItemWithoutModel(string name)
        {
            TItemViewModel item = GetItemByName(name);
            return TryRemoveItemWithoutModel(item, true);
        }

        public TItemViewModel GetItemByName(string name)
        {
            return items.Find(x => x.Name == name);
        }

        protected abstract bool TryRemoveModelWithoutRecord(string name, TItemModel model);
    }
}