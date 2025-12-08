// <copyright file="NameTypeItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public class
        NameTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView> : BaseTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, INameViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        private TextField nameField;

        public NameTypeItemCreatorView() : base(UIBuilderResourcePaths.NameTypeItemCreatorView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            nameField = this.Q<TextField>("NameField");
            nameField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string previousName = evt.previousValue;
                string newName = evt.newValue;

                if (!string.IsNullOrEmpty(newName) && !NameValidator.ValidateName(newName))
                {
                    newName = previousName;
                    nameField.SetValueWithoutNotify(newName);
                }

                ValidateCreateButtonByName(newName);
            });
        }

        protected void ValidateCreateButtonByName(string name)
        {
            bool isValid = ValidateName(name);
            createButton.SetEnabled(isValid);
        }

        private bool ValidateName(string name)
        {
            if (IsRuntime) return false;

            if (string.IsNullOrEmpty(name))
                return false;

            if (ViewModel == null || ViewModel.Contains(name))
                return false;

            return true;
        }

        protected override void OnRegisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
        }

        private void OnItemAdded(TItemViewModel item)
        {
            ValidateCreateButtonByName(nameField.text);
        }

        private void OnItemRemoved(TItemViewModel item)
        {
            ValidateCreateButtonByName(nameField.text);
        }

        protected override void CreateNewItem()
        {
            Type type = TypeField.value;
            string name = nameField.text;
            if (type != null) ViewModel.CreateItem(type, name);
        }
    }
}