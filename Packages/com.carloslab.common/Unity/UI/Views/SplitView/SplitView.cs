// <copyright file="SplitView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using System;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class SplitView<TMainView, TSubView, TRootView> : TwoPaneSplitView, IRootViewMember<TRootView>
        where TMainView : BaseView, IMainView<TSubView>, IRootViewMember<TRootView>, new()
        where TSubView : BaseView, IRootViewMember<TRootView>, new()
        where TRootView : BaseView, IRootView
    {
        public SplitView()
        {
            MainView = new();
            MainView.style.minWidth = 100;

            SubView = new TSubView();
            // SubView.style.minWidth = 100;

            MainView.InitSubView(SubView);

            Add(MainView);
            Add(SubView);

            fixedPaneIndex = 0;
            fixedPaneInitialDimension = 300;

            RegisterCallback<AttachToPanelEvent>(AttachToPanelEventHandler);
            fixedPane.RegisterCallback<GeometryChangedEvent>(FixedPane_GeometryChangedEventHandler);
        }

        #region SplitView

        public TMainView MainView { get; }
        public TSubView SubView { get; }

        private void AttachToPanelEventHandler(AttachToPanelEvent evt)
        {
            OnAttachToPanel(evt);
        }

        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
        }

        private void FixedPane_GeometryChangedEventHandler(GeometryChangedEvent evt)
        {
            FixedPane_OnGeometryChanged(evt);
        }

        protected virtual void FixedPane_OnGeometryChanged(GeometryChangedEvent evt)
        {
        }

        #endregion

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

                HandleRootViewChanged(rootView);
            }
        }

        private void HandleRootViewChanged(TRootView rootView)
        {
            MainView.RootView = rootView;
            SubView.RootView = rootView;
            OnRootViewChanged(rootView);
        }

        protected virtual void OnRootViewChanged(TRootView rootView)
        {

        }

        #endregion

        #region Show

        public event Action Shown;

        public void Show(bool show)
        {
            this.SetDisplay(show);
            if (show)
                RaiseShownEvent();
            else
                RaiseHiddenEvent();
        }

        public void RaiseShownEvent()
        {
            Shown?.Invoke();
            OnShown();
        }

        protected virtual void OnShown()
        {
        }

        #endregion

        #region Hide

        public event Action Hidden;

        public void RaiseHiddenEvent()
        {
            Hidden?.Invoke();
            OnHidden();
        }

        protected virtual void OnHidden()
        {
        }

        #endregion

    }
}