// <copyright file="NameItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class
        NameItemCreatorView<TListViewModel, TItemViewModel, TRootView> : BaseItemCreatorView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, INameViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        private TextField nameField;

        public NameItemCreatorView(string visualAssetPath = UIBuilderResourcePaths.NameItemCreatorView) : base(
            visualAssetPath)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            nameField = this.Q<TextField>("NameField");
            nameField.label = "Name";
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

        protected override void CreateNewItem()
        {
            ViewModel.CreateItem(null, nameField.text);
        }

        protected override void OnRegisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
            viewModel.ItemRemoved -= OnItemRemoved;
        }

        protected virtual void OnItemAdded(TItemViewModel item)
        {
            ValidateCreateButtonByName(nameField.text);
        }

        protected virtual void OnItemRemoved(TItemViewModel item)
        {
            ValidateCreateButtonByName(nameField.text);
        }
    }
}