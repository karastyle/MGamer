// <copyright file="Decision.Item.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision
    {
        #region IContainerItem

        private string name;
        public override string Name => name;

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
        }

        public bool IsInContainer => container != null;
        private DecisionContainer container;
        public DecisionContainer Container => container;

        void IContainerItem.HandleItemAdded(IItemContainer container, string name)
        {
            this.container = container as DecisionContainer;
            this.name = name;
        }

        void IContainerItem.HandleItemRemoved()
        {
            this.container = null;
        }

        #endregion
    }
}