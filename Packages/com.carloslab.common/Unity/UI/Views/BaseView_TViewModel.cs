// <copyright file="BaseView_TViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public abstract class BaseView<TViewModel> : BaseView, IView<TViewModel>
        where TViewModel : class, IViewModel
    {
        protected BaseView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        #region ViewModel

        private TViewModel viewModel;


        public TViewModel ViewModel
        {
            get => viewModel;
            protected set
            {
                if (viewModel == value)
                    return;

                UnregisterViewModelEvents(viewModel);

                viewModel = value;

                RegisterViewModelEvents(viewModel);

                HandleViewModelChanged(viewModel);
            }
        }

        public event Action<TViewModel> ViewModelChanged;

        #endregion

        #region UpdateView

        public virtual void UpdateView(TViewModel viewModel)
        {
            ViewModel = viewModel;

            if (viewModel != null)
            {
                dataSource = viewModel;
                OnRefreshView(viewModel);
            }
            else
            {
                dataSource = null;
                OnResetView();
            }

            OnUpdateView(viewModel);
        }

        protected virtual void OnUpdateView(TViewModel viewModel)
        {
        }

        protected virtual void OnRefreshView(TViewModel viewModel)
        {
        }

        protected virtual void OnResetView()
        {

        }

        #endregion

        #region ViewModel Events

        private void HandleViewModelChanged(TViewModel viewModel)
        {
            OnViewModelChanged(viewModel);
            ViewModelChanged?.Invoke(viewModel);
        }

        private void RegisterViewModelEvents(TViewModel viewModel)
        {
            if (this.viewModel == null) return;

            viewModel.ModelChanged += OnModelChanged;
            OnRegisterViewModelEvents(viewModel);
        }

        private void UnregisterViewModelEvents(TViewModel viewModel)
        {
            if (viewModel == null)
                return;

            viewModel.ModelChanged -= OnModelChanged;
            OnUnregisterViewModelEvents(viewModel);
        }

        protected virtual void OnRegisterViewModelEvents(TViewModel viewModel)
        {
        }

        protected virtual void OnUnregisterViewModelEvents(TViewModel viewModel)
        {
        }

        protected virtual void OnViewModelChanged(TViewModel viewModel)
        {
        }

        protected virtual void OnModelChanged(IModel newModel)
        {
        }

        #endregion

    }
}