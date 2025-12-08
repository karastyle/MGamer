// <copyright file="ListViewCustomBinding.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public class ListViewCustomBinding : CustomBinding, IDataSourceProvider
    {
        private readonly Dictionary<BaseListView, int> cachedCount = new();

        public object dataSource { get; }
        public PropertyPath dataSourcePath { get; set; }

        protected override void OnActivated(in BindingActivationContext context)
        {
            // Debug.Log($"ListViewCustomBinding OnActivated targetType: {context.targetElement.GetType().Name}");
            if (context.targetElement is not BaseListView listView)
                return;

            // Ensures the refresh will be called on the next update
            cachedCount[listView] = -1;
        }

        protected override void OnDeactivated(in BindingActivationContext context)
        {
            // Debug.Log($"ListViewCustomBinding OnDeactivated targetType: {context.targetElement.GetType().Name}");

            if (context.targetElement is not BaseListView listView)
                return;

            cachedCount.Remove(listView);
        }

        protected override BindingResult Update(in BindingContext context)
        {
            if (context.targetElement is not BaseListView listView)
            {
                return new BindingResult(BindingStatus.Failure,
                    "'ListViewCustomBinding' should only be added to a 'ListView'");
            }

            PropertyContainer.TryGetValue(context.dataSource, context.dataSourcePath,
                out IList itemsSource);

            listView.itemsSource = itemsSource;
            int currentCount = itemsSource?.Count ?? -1;

            if (!cachedCount.TryGetValue(listView, out int previousCount) || previousCount == currentCount)
                return new BindingResult(BindingStatus.Failure, string.Empty);

            // Debug.Log($"ListViewCustomBinding Update targetType: {context.targetElement.GetType().Name}");

            if (previousCount < currentCount)
                listView.RefreshItems();
            else
                listView.Rebuild();

            cachedCount[listView] = currentCount;

            return new BindingResult(BindingStatus.Success);
        }
    }
}