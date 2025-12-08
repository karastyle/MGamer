// <copyright file="BaseNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class BaseNameItemView<TItemViewModel, TRootView> : BaseItemView<TItemViewModel, TRootView>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel
        where TRootView : BaseView, IRootView
    {
        private readonly Label nameLabel;
        private TextField renameField;

        private bool enableRename;
        private bool enableRemove;

        public BaseNameItemView(bool enableRename, bool enableRemove = true) : base(null)
        {
            nameLabel = new();
            nameLabel.focusable = true;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.flexGrow = 1.0f;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(nameLabel);

            this.enableRename = enableRename;
            this.enableRemove = enableRemove;
        }

        protected Label NameLabel => nameLabel;

        #region Delete Function

        protected void AddRemoveFunction()
        {
            nameLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Remove", menuAction => { RemoveFromList(); });
            }));
        }

        #endregion

        protected override void OnEnableEditMode()
        {
            if (enableRemove) AddRemoveFunction();
            if (enableRename) AddRenameFunction();
        }

        #region Rename Function

        protected void AddRenameFunction()
        {
            if (renameField != null) return;

            CreateRenameField();

            nameLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2) EnableRenameMode();
            });

            nameLabel.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.F2)
                {
                    if (ViewModel.IsSelected)
                        EnableRenameMode();
                }
            });

            nameLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.InsertAction(0, "Rename", menuAction => { EnableRenameMode(); });
            }));
        }

        private void CreateRenameField()
        {
            renameField = new TextField();
            renameField.style.flexGrow = 1;
            renameField.SetDisplay(false);

            renameField.RegisterCallback<FocusOutEvent>(evt =>
            {
                DisableRenameMode();
                string newName = renameField.text;

                if (!string.IsNullOrEmpty(newName))
                    RenameItem(newName);
            });

            Add(renameField);
        }

        private void RenameItem(string newName)
        {
            ViewModel.RenameItem(newName);
        }

        private void EnableRenameMode()
        {
            nameLabel.SetDisplay(false);

            renameField.SetValueWithoutNotify(NameLabel.text);
            renameField.SetDisplay(true);

            renameField.focusable = true;
            renameField.schedule.Execute(() => renameField.Focus()).StartingIn(100);
        }

        private void DisableRenameMode()
        {
            nameLabel.SetDisplay(true);

            renameField.SetDisplay(false);
        }

        #endregion
    }
}