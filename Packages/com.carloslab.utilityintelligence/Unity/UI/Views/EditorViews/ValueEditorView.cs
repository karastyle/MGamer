// <copyright file="ValueEditorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ValueEditorView<TViewModel> : UtilityIntelligenceViewMember<TViewModel>
        where TViewModel : class, IValueViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        private ValueField valueField;

        public ValueEditorView() : base(null)
        {
        }

        protected override void OnRefreshView(TViewModel viewModel)
        {
            UpdateValueField(viewModel);
        }

        protected override void OnResetView()
        {
            RemoveValueField();
        }

        private void UpdateValueField(TViewModel viewModel)
        {
            RemoveValueField();
            CreateValueField(viewModel);
        }

        private void CreateValueField(TViewModel viewModel)
        {
            if (viewModel == null) return;

            var valueField = new ValueField(viewModel.ValueType, false, "Value");
            if (valueField.IsValid == false) return;

            valueField.SetDataBinding(nameof(ValueField.Value), nameof(IValueViewModel.ValueObject),
                BindingMode.TwoWay);

            Add(valueField);

            this.valueField = valueField;
        }

        private void RemoveValueField()
        {
            if (valueField != null)
            {
                Remove(valueField);
                valueField = null;
            }
        }
    }
}