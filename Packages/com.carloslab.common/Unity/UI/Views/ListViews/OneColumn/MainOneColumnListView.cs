// <copyright file="MainOneColumnListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public abstract class MainOneColumnListView<TListViewModel, TItemViewModel, TSubView, TRootView> :
        OneColumnListView<TListViewModel, TItemViewModel, TRootView>, IMainView<TSubView>
        where TListViewModel : class, IRootViewModelMember, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel
        where TSubView : BaseView
        where TRootView : BaseView, IRootView
    {
        protected MainOneColumnListView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        public TSubView SubView { get; private set; }

        public void InitSubView(TSubView subView)
        {
            SubView = subView;
            OnInitSubView(subView);
        }

        protected virtual void OnInitSubView(TSubView subView)
        {
        }

        protected sealed override void OnSelectedIndexChanged(int index)
        {
            ViewModel.SelectedIndex = index;
        }

        protected sealed override void OnItemsSourceChanged()
        {
            ResetSelectionWhenItemsSourceChanged();
        }

        protected sealed override void OnItemAdded(TItemViewModel newItem)
        {
            SelectFirstItemIfOnlyOneItemExists();
        }

        protected sealed override void OnItemRemoved(TItemViewModel item)
        {
            SelectFirstItemIfOnlyOneItemExists();

            UpdateSelectionWhenItemIndexesChanged();
        }

        #region ListView Selection

        private void SelectFirstItemIfOnlyOneItemExists()
        {
            if (SelectedIndex < 0 && Items.Count == 1) SelectedIndex = 0;
        }

        private void ResetSelectionWhenItemsSourceChanged()
        {
            int selectedIndex = ViewModel.SelectedIndex;
            if (selectedIndex < 0 && Items.Count > 0)
                selectedIndex = 0;

            ClearSelection();
            SelectedIndex = selectedIndex;
        }

        private void UpdateSelectionWhenItemIndexesChanged()
        {
            if (SelectedItem == null)
                return;

            int selectedIndex = SelectedItem.Index;
            if (SelectedIndex != selectedIndex)
                SelectedIndex = selectedIndex;
        }

        #endregion
    }
}