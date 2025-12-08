// <copyright file="GenericModelItem.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class GenericModelItem<TContainer, TRuntime> : GenericModelWithId<TRuntime>, IContainerItem
        where TContainer : class, IItemContainer
        where TRuntime : class, IRuntimeObject, IContainerItem
    {
        #region IContainerItem

        public string Name
        {
            get => GetValue(nameof(Name)) as string;
            internal set => SetValue(nameof(Name), value);
        }

        string IContainerItem.Name
        {
            get => GetValue(nameof(IContainerItem.Name)) as string;
            set => SetValue(nameof(IContainerItem.Name), value);
        }

        public bool IsInContainer => container != null;

        private TContainer container;
        public TContainer Container => container;

        void IContainerItem.HandleItemAdded(IItemContainer container, string name)
        {
            this.container = container as TContainer;
            Name = name;
            OnItemAdded();
        }

        void IContainerItem.HandleItemRemoved()
        {
            this.container = null;
            OnItemRemoved();
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