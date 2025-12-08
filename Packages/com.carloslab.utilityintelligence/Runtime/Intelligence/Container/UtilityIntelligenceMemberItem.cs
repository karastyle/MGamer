// <copyright file="UtilityIntelligenceMemberItem.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class UtilityIntelligenceMemberItem<TContainer> : UtilityIntelligenceMember, IContainerItem
        where TContainer : class, IItemContainer
    {
        #region IModelWithId

        [DataMember(Name = nameof(Id))]
        private string id;
        public string Id => id;

        #endregion

        #region IContainerItem

        [DataMember(Name = nameof(Name))]
        private string name;

        public string Name
        {
            get => name;
            internal set => name = value;
        }

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
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

        protected virtual void OnItemAdded()
        {

        }

        protected virtual void OnItemRemoved()
        {

        }

        #endregion
    }
}