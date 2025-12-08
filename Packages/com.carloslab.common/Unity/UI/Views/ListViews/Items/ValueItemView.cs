// <copyright file="ValueItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public class ValueItemView<TViewModel, TRootView> : BaseItemView<TViewModel, TRootView>
        where TViewModel : class, IItemViewModel, IValueViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        private ValueField valueField;

        public ValueItemView() : base(null)
        {
        }

        public ValueField ValueField => valueField;

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

        private void RemoveValueField()
        {
            if (valueField != null)
            {
                Remove(valueField);
                valueField = null;
            }
        }

        private void CreateValueField(TViewModel viewModel)
        {
            if (viewModel == null) return;

            var valueField = new ValueField(viewModel.ValueType, true);

            if (valueField.IsValid == false) return;

            valueField.SetEnabled(false);

            Add(valueField);

            this.valueField = valueField;
        }

        protected void DisableValueField()
        {
            if (valueField == null) return;

            valueField.SetEnabled(false);
        }

        protected void EnableValueField()
        {
            if (valueField == null) return;

            valueField.SetEnabled(true);
        }

        protected void SetValueFieldBinding()
        {
            if (valueField == null) return;

            valueField.SetValueDataBinding(nameof(IValueViewModel.ValueObject), BindingMode.TwoWay);
        }

        protected void ClearValueFieldBinding()
        {
            if (valueField == null) return;

            valueField.ClearBindings();
        }
    }
}