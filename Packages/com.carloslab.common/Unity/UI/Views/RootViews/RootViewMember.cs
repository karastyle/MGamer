// <copyright file="RootViewMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public abstract class RootViewMember<TRootView> : BaseView, IRootViewMember<TRootView>
        where TRootView : BaseView, IRootView
    {
        protected RootViewMember(string visualAssetPath) : base(visualAssetPath)
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
    public abstract class RootViewMember<TViewModel, TRootView> : BaseView<TViewModel>, IRootViewMember<TRootView>
        where TViewModel : class, IViewModel
        where TRootView : BaseView, IRootView
    {
        public RootViewMember(string visualAssetPath) : base(visualAssetPath)
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

                HandleRootViewChanged(rootView);
            }
        }

        protected virtual void HandleRootViewChanged(TRootView rootView)
        {
            OnRootViewChanged(rootView);
        }

        protected virtual void OnRootViewChanged(TRootView rootView)
        {

        }

        #endregion

        public sealed override void UpdateView(TViewModel viewModel)
        {
            base.UpdateView(viewModel);

            UpdateRuntimeMode();
        }

        protected void UpdateRuntimeMode()
        {
            if (IsRuntime)
            {
                OnEnableRuntimeMode();
            }
            else
            {
                OnEnableEditMode();
            }
        }

        protected virtual void OnEnableEditMode()
        {
        }

        protected virtual void OnEnableRuntimeMode()
        {
        }
    }
}