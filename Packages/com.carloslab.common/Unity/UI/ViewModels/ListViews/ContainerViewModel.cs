// <copyright file="ContainerViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.Common.UI
{
    public abstract class
        ContainerViewModel<TModel, TItemModel, TItemViewModel, TRootViewModel> : BaseListViewModel<TModel, TItemModel, TItemViewModel, TRootViewModel>, INameListViewModel
        where TModel : class, IItemContainerModel<TItemModel>
        where TItemModel : class, IModelWithId, IContainerItem
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, INameViewModel
        , IRootViewModelMember<TRootViewModel>, new()
        where TRootViewModel : class, IRootViewModel
    {
        public override IReadOnlyList<TItemModel> ItemModels => Model?.Items;
        public event Action<TItemViewModel, string, string> ItemNameChanged;

        #region Event Functions

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveItem(sourceIndex, destIndex);
        }

        #endregion

        #region Items

        public TItemViewModel GetItemByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return items.Find(x => x.Name == name);
        }

        public override bool TryRenameItem(TItemViewModel item, string newName)
        {
            if (!Contains(newName))
            {
                string oldName = item.Name;

                item.Name = newName;

                if (Model.TryRemoveItem(oldName) && Model.TryAddItem(item.Index, newName, item.Model))
                {
                    ItemNameChanged?.Invoke(item, oldName, newName);
                    return true;
                }
            }

            return false;
        }

        public override TItemViewModel CreateItem(Type modelType, object itemInfo)
        {
            string name = itemInfo as string;
            if (Model.Contains(name))
                return null;

            TItemModel model = CreateModel(modelType);
            model.Name = name;

            TryCreateItem(model, out TItemViewModel item);
            return item;
        }

        public bool Contains(string name)
        {
            return Model.Contains(name);
        }

        #endregion

        #region Models

        protected override bool TryAddModelWithoutRecord(TItemModel model, int index)
        {
            return Model.TryAddItem(index, model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(TItemModel model)
        {
            return Model.TryRemoveItem(model.Name);
        }

        #endregion
    }
}