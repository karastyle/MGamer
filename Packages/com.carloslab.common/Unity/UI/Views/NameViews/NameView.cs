// <copyright file="NameView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class NameView<TViewModel, TRootView>
        : RootViewMember<TViewModel, TRootView>
        where TViewModel : class, INameViewModel
        where TRootView : BaseView, IRootView
    {

        private ScrollView container;
        public ScrollView Container => container;
        private TextField nameField;

        private Label titleLabel;
        public Label TitleLabel => titleLabel;

        protected NameView(string visualAssetPath) : base(visualAssetPath)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            titleLabel = this.Q<Label>("TitleLabel");
            container = this.Q<ScrollView>("Container");
            nameField = this.Q<TextField>("NameField");

            nameField.textEdition.placeholder = "Name...";
            nameField.SetEnabled(false);
        }

        protected override void OnRefreshView(TViewModel viewModel)
        {
            nameField.SetDataBinding(nameof(TextField.value), nameof(INameViewModel.Name), BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            nameField.ClearBindings();
        }
    }
}