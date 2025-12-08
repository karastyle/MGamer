// <copyright file="RootViewModelMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public class RootViewModelMember<TModel, TRootViewModel> : ViewModel<TModel>, IRootViewModelMember<TRootViewModel>
        where TModel : class, IModel
        where TRootViewModel : class, IRootViewModel
    {
        public int DataVersion => rootViewModel?.DataVersion ?? 0;
        public bool IsEditorOpening => rootViewModel?.IsEditorOpening ?? false;
        public bool IsRuntime => rootViewModel?.IsRuntime ?? false;
        public void Record(string name, Action action, bool save = false)
        {
            rootViewModel?.Record(name, action, save);
        }

        private TRootViewModel rootViewModel;

        public TRootViewModel RootViewModel
        {
            get => rootViewModel;
            set
            {
                if (rootViewModel == value) return;

                UnregisterRootViewModelEvents(rootViewModel);

                rootViewModel = value;

                RegisterRootViewModelEvents(rootViewModel);

                HandleRootViewModelChanged(rootViewModel);

            }
        }

        protected virtual void HandleRootViewModelChanged(TRootViewModel rootViewModel)
        {
            OnRootViewModelChanged(rootViewModel);
        }

        protected virtual void OnRootViewModelChanged(TRootViewModel rootViewModel)
        {

        }

        private void UnregisterRootViewModelEvents(TRootViewModel viewModel)
        {
            if (viewModel != null)
                OnUnregisterRootViewModelEvents(viewModel);
        }

        protected virtual void OnUnregisterRootViewModelEvents(TRootViewModel viewModel)
        {

        }

        private void RegisterRootViewModelEvents(TRootViewModel viewModel)
        {
            if (viewModel != null)
                OnRegisterRootViewModelEvents(viewModel);
        }
        protected virtual void OnRegisterRootViewModelEvents(TRootViewModel viewModel)
        {

        }
    }
}