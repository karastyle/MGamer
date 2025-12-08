// <copyright file="NoTargetContainer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public class NoTargetContainer<TItem> : UtilityIntelligenceMemberContainer<TItem>
        where TItem : class, IUtilityIntelligenceMember, INoTargetItem
    {
        private List<TItem> noTargetItems = new();
        internal List<TItem> NoTargetItems => noTargetItems;

        protected override void OnItemAdded(TItem item)
        {
            base.OnItemAdded(item);

            if (item.HasNoTarget)
                noTargetItems.Add(item);
        }

        protected override void OnItemRemoved(TItem item)
        {
            base.OnItemRemoved(item);

            if (item.HasNoTarget)
                noTargetItems.Remove(item);
        }
    }
}