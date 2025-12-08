// <copyright file="InputListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputListView : NameValueListView<InputListViewModel, InputItemViewModel>, IMainView<InputTabSubView>
    {
        public InputTabSubView SubView { get; private set; }

        public void InitSubView(InputTabSubView subView)
        {
            SubView = subView;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowInputEditorView(SelectedItem);
            else
                SubView.HideInputEditorView();
        }

        #region Make Cells

        protected override VisualElement MakeCellName()
        {
            return new InputNameItemView();
        }

        protected override VisualElement MakeCellValue()
        {
            return new InputValueItemView();
        }

        protected override void BindCellValue(VisualElement element, int index)
        {
            InputValueItemView itemView = element as InputValueItemView;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        #endregion
    }
}