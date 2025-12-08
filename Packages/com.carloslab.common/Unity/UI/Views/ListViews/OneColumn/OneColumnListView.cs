// <copyright file="OneColumnListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class
        OneColumnListView<TListViewModel, TItemViewModel, TRootView> : BaseListView<TListViewModel, TItemViewModel, ListView, TRootView>
        where TListViewModel : class, IRootViewModelMember, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel
        where TRootView : BaseView, IRootView
    {
        protected OneColumnListView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        protected sealed override void InitListView(ListView listView)
        {
            listView.makeItem = ListView_OnMakeItem;
            listView.bindItem = ListView_OnBindItem;
            listView.destroyItem = ListView_OnDestroyItem;

            base.InitListView(listView);
        }

        private VisualElement ListView_OnMakeItem()
        {
            var item = OnMakeItem();

            if (item is IRootViewMember<TRootView> rootViewMember)
                rootViewMember.RootView = RootView;

            return item;
        }

        private void ListView_OnDestroyItem(VisualElement item)
        {
            if (item is IRootViewMember<TRootView> rootViewMember)
                rootViewMember.RootView = null;
        }

        private void ListView_OnBindItem(VisualElement element, int index)
        {
            if (index >= ViewModel.Items.Count || index < 0)
                return;

            OnBindItem(element, index);
        }

        protected abstract VisualElement OnMakeItem();
        protected abstract void OnBindItem(VisualElement element, int index);
    }
}