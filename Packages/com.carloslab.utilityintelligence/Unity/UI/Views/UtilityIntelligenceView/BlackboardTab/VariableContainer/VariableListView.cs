// <copyright file="VariableListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableListView : NameValueListView<BlackboardViewModel, VariableViewModel>, IMainView<BlackboardTabSubView>
    {
        #region Make Cells

        protected override VisualElement MakeCellName()
        {
            return new VariableNameItemView();
        }

        protected override VisualElement MakeCellValue()
        {
            return new VariableValueItemView();
        }

        #endregion

        public BlackboardTabSubView SubView { get; private set; }

        public void InitSubView(BlackboardTabSubView subView)
        {
            SubView = subView;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowVariableView(SelectedItem);
            else
                SubView.HideVariableView();
        }
    }
}