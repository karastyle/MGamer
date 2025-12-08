// <copyright file="ContainerItem.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    [DataContract]
    public abstract class ContainerItem<TContainer> : ModelWithId, IContainerItem
    where TContainer : class, IItemContainer
    {
        [DataMember(Name = nameof(Name))]
        private string name;

        public string Name
        {
            get => name;
            internal set => name = value;
        }

        public bool IsInContainer => container != null;

        private TContainer container;
        public TContainer Container => container;
        void IContainerItem.HandleItemAdded(IItemContainer container, string name)
        {
            this.container = container as TContainer;
            this.name = name;
            OnItemAdded();
        }

        void IContainerItem.HandleItemRemoved()
        {
            this.container = null;
            OnItemRemoved();
        }

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
        }

        protected virtual void OnItemAdded()
        {

        }

        protected virtual void OnItemRemoved()
        {

        }
    }
}