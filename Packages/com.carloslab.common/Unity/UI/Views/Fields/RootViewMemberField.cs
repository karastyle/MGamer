// <copyright file="RootViewMemberField.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public class RootViewMemberField<TValueType, TRootView> : CustomField<TValueType>, IRootViewMember<TRootView>
        where TRootView : BaseView, IRootView
    {
        public RootViewMemberField(string label, VisualElement input, string visualAssetPath) : base(label, input, visualAssetPath)
        {
        }

        public RootViewMemberField(string label, string visualAssetPath) : base(label, visualAssetPath)
        {
        }

        #region IRootViewMember

        public bool IsRuntime => rootView?.IsRuntime ?? false;
        public bool IsRuntimeUI => rootView?.IsRuntimeUI ?? false;

        private TRootView rootView;

        public TRootView RootView
        {
            get => rootView;
            set
            {
                if (rootView == value) return;

                rootView = value;

                OnRootViewChanged(rootView);
            }
        }

        protected virtual void OnRootViewChanged(TRootView rootView)
        {

        }

        #endregion
    }
}