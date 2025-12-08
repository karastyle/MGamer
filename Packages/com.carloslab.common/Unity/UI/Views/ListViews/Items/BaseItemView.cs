// <copyright file="BaseItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class BaseItemView<TItemViewModel, TRootView> : RootViewMember<TItemViewModel, TRootView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        private VisualElement itemContainerView;

        private RemoveButton removeButton;

        public RemoveButton RemoveButton => removeButton;

        public BaseItemView(string visualAssetPath) : base(visualAssetPath)
        {
            style.flexGrow = 1.0f;
        }

        public int Index => ViewModel.Index;

        public VisualElement ItemContainerView
        {
            get
            {
                if (itemContainerView == null)
                {
                    itemContainerView = this.GetFirstAncestorWithClass(BaseListView.itemUssClassName);
                    if (itemContainerView != null)
                    {
                        InitItemContainerView(itemContainerView);
                    }
                }


                return itemContainerView;
            }
        }

        protected virtual void InitItemContainerView(VisualElement itemContainerView)
        {

        }

        protected void RemoveFromList()
        {
            ViewModel.RemoveFromList();
        }

        protected RemoveButton CreateRemoveButton()
        {
            removeButton = new();
            // deleteButton.style.position = Position.Absolute;
            // deleteButton.style.right = 0;
            removeButton.clicked += () => RemoveFromList();
            Add(removeButton);
            return removeButton;
        }

        protected override void OnRootViewChanged(TRootView rootView)
        {
            removeButton?.UpdateView(IsRuntimeUI);
        }
    }
}