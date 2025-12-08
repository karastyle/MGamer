// <copyright file="RootViewValueField.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public class RootViewValueField<TRootView> : ValueField, IRootViewMember<TRootView>
        where TRootView : BaseView, IRootView
    {
        public RootViewValueField(Type valueType, bool isDelayed = false, string label = null) : base(valueType, isDelayed, label)
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