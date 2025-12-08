// <copyright file="BaseItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class
        BaseItemCreatorView<TListViewModel, TItemViewModel, TRootView> : RootViewMember<TListViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView

    {
        protected Button createButton;

        protected virtual string CreateButtonText { get; } = "Create";

        public BaseItemCreatorView(string visualAssetPath) : base(visualAssetPath)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            createButton = this.Q<Button>("CreateButton");
            createButton.text = CreateButtonText;
            createButton.clicked += OnCreateButtonClicked;
            createButton.SetEnabled(false);
        }

        private void OnCreateButtonClicked()
        {
            CreateNewItem();
        }

        protected abstract void CreateNewItem();
    }
}