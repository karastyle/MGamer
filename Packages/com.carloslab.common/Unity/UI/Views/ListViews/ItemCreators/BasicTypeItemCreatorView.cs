// <copyright file="BasicTypeItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public abstract class BasicTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView> : BaseTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        protected override void OnTypeFieldValueChanged(Type newType)
        {
            bool isValidated = ValidateType(newType);
            createButton.SetEnabled(isValidated);
        }

        private bool ValidateType(Type type)
        {
            if (IsRuntime
                || type == null
                || ViewModel == null) return false;

            return true;
        }
    }
}