// <copyright file="NameValueListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class NameValueListView<TListViewModel, TItemViewModel, TRootView> :
        BasicListView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, INameViewModel, IValueViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {

        protected NameValueListView() : base(null)
        {
        }

        #region MultiColumns

        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterValueColumn(listView);
            RegisterControlsColumn(listView);
        }

        protected override VisualElement OnMakeCell(string columnName)
        {
            VisualElement cell = null;
            switch (columnName)
            {
                case ValueColumnName:
                    cell = MakeCellValue();
                    break;
                default:
                    cell = base.OnMakeCell(columnName);
                    break;
            }

            return cell;
        }

        protected override void OnBindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case ValueColumnName:
                    BindCellValue(element, index);
                    break;
                default:
                    base.OnBindCell(columnName, element, index);
                    break;
            }
        }

        protected override void OnUnbindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case ValueColumnName:
                    UnbindCellValue(element, index);
                    break;
                default:
                    base.OnUnbindCell(columnName, element, index);
                    break;
            }
        }

        #endregion

        #region ValueColumn

        private const string ValueColumnName = "Value";

        private void RegisterValueColumn(MultiColumnListView listView)
        {
            Column valueColumn = RegisterColumn(listView, ValueColumnName, ValueColumnName, 1);
            valueColumn.stretchable = true;
        }

        protected virtual VisualElement MakeCellValue()
        {
            return new ValueItemView<TItemViewModel, TRootView>();
        }

        protected virtual void BindCellValue(VisualElement element, int index)
        {
            ValueItemView<TItemViewModel, TRootView> itemView = element as ValueItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        private void UnbindCellValue(VisualElement element, int index)
        {
            var itemView = element as ValueItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(null);
        }

        #endregion

    }
}