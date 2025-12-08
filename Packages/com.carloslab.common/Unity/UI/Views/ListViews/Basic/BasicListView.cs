// <copyright file="BasicListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class BasicListView<TListViewModel, TItemViewModel, TRootView> :
        MultiColumnListView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {

        protected BasicListView(string visualAssetPath) : base(visualAssetPath)
        {

        }

        #region NameColumn

        private const string NameColumnName = "Name";
        protected virtual string NameColumnTitle => "Name";

        protected void RegisterNameColumn(MultiColumnListView listView)
        {
            Column nameColumn = RegisterColumn(listView, NameColumnName, NameColumnTitle);
            nameColumn.stretchable = true;
        }

        protected abstract VisualElement MakeCellName();

        protected void BindCellName(VisualElement element, int index)
        {
            BaseNameItemView<TItemViewModel, TRootView> itemView = element as BaseNameItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        private void UnbindCellName(VisualElement element, int index)
        {
            var itemView = element as BaseNameItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(null);
        }

        #endregion

        #region Controls Column

        private const string ControlsColumnName = "Controls";


        protected void RegisterControlsColumn(MultiColumnListView listView)
        {
            RegisterColumn(listView, ControlsColumnName, string.Empty);
        }

        protected virtual VisualElement MakeCellControls()
        {
            if (!IsRuntime)
                return new ControlsItemView<TItemViewModel, TRootView>();
            else
                return new VisualElement();
        }

        private void BindCellControls(VisualElement element, int index)
        {
            if (element is ControlsItemView<TItemViewModel, TRootView> itemView)
            {
                var viewModel = ViewModel.Items[index];
                itemView.UpdateView(viewModel);
            }
        }

        private void UnbindCellControls(VisualElement element, int index)
        {
            if (element is ControlsItemView<TItemViewModel, TRootView> itemView)
            {
                itemView.UpdateView(null);
            }
        }

        #endregion

        #region MultiColumn

        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterControlsColumn(listView);
        }

        protected override VisualElement OnMakeCell(string columnName)
        {
            VisualElement cell = null;
            switch (columnName)
            {
                case NameColumnName:
                    cell = MakeCellName();
                    break;
                case ControlsColumnName:
                    cell = MakeCellControls();
                    break;
            }

            return cell;
        }

        protected override void OnBindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case NameColumnName:
                    BindCellName(element, index);
                    break;
                case ControlsColumnName:
                    BindCellControls(element, index);
                    break;
            }
        }

        protected override void OnUnbindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case NameColumnName:
                    UnbindCellName(element, index);
                    break;
                case ControlsColumnName:
                    UnbindCellControls(element, index);
                    break;
            }
        }

        #endregion
    }
}