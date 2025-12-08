// <copyright file="ItemReference.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public abstract class ItemReference<TItem, TContainer>
        where TItem : class, IContainerItem
        where TContainer : ItemContainer<TItem>
    {
        private string name;
        private TContainer container;

        private TItem item;

        public ItemReference()
        {

        }

        public ItemReference(string name, TContainer container)
        {
            this.name = name;
            this.container = container;
        }

        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                OnNameChanged();
            }
        }

        public TContainer Container
        {
            get => container;
            set
            {
                if (container == value) return;

                container = value;
                OnContainerChanged();
            }
        }

        public TItem Item
        {
            get
            {
                if (item == null)
                    UpdateItem();

                return item;
            }
        }


        private TItem GetItem(string name)
        {
            if (string.IsNullOrEmpty(name) || container == null)
                return null;
            return container.GetItem(name);
        }

        private void UpdateItem()
        {
            item = GetItem(name);
        }

        private void OnNameChanged()
        {
            UpdateItem();
        }

        private void OnContainerChanged()
        {
            UpdateItem();
        }
    }
}