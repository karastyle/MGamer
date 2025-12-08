// <copyright file="DecisionMakerItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerItemViewModel : BaseItemViewModel<DecisionMakerModel, DecisionMakerListViewModel>, INameViewModel,
        IStatusViewModel, INotifyBindablePropertyChanged
    {
        public event Action<DecisionMakerContext> ContextChanged;

        protected override void OnInit(DecisionMakerModel model)
        {
            contextViewModel.Init(model);
            RegisterContextViewModelEvents(contextViewModel);

            decisionListViewModel.Init(model, this);
        }

        protected override void OnDeinit()
        {
            UnregisterContextViewModelEvents(contextViewModel);
            contextViewModel.Deinit();
            contextViewModel = null;

            decisionListViewModel.Deinit();
            decisionListViewModel = null;
        }

        private void RegisterContextViewModelEvents(DecisionMakerContextViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.ContextChanged += ContextViewModel_OnContextChanged;
        }

        private void UnregisterContextViewModelEvents(DecisionMakerContextViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.ContextChanged -= ContextViewModel_OnContextChanged;
        }

        private void ContextViewModel_OnContextChanged(DecisionMakerContext context)
        {
            ContextChanged?.Invoke(context);
        }

        protected override void OnRootViewModelChanged(UtilityIntelligenceViewModel rootViewModel)
        {
            contextViewModel.RootViewModel = RootViewModel;
            decisionListViewModel.RootViewModel = rootViewModel;
        }

        #region ViewModels

        private DecisionMakerContextViewModel contextViewModel = new();

        public DecisionMakerContextViewModel ContextViewModel => contextViewModel;

        private DecisionListViewModelIntelligenceTab decisionListViewModel = new();
        public DecisionListViewModelIntelligenceTab DecisionListViewModel => decisionListViewModel;



        #endregion

        #region Status

        public Status CurrentStatus => Model.Runtime.CurrentStatus;
        public event Action<Status> StatusChanged;

        private void OnStatusChanged(Status newStatus)
        {
            StatusChanged?.Invoke(newStatus);
        }

        #endregion

        #region Model

        protected override void OnRegisterModelEvents(DecisionMakerModel model)
        {
            if (IsRuntime)
                model.Runtime.StatusChanged += OnStatusChanged;
        }

        protected override void OnUnregisterModelEvents(DecisionMakerModel model)
        {
            model.Runtime.StatusChanged -= OnStatusChanged;
        }

        protected override void OnModelChanged(DecisionMakerModel newModel)
        {
            ContextViewModel.Model = newModel;
            DecisionListViewModel.Model = newModel;

            if (newModel == null) return;

            Notify(nameof(Name));
        }

        #endregion

        #region DecisionMaker

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value)
                    return;

                Record($"DecisionMakerViewModel Name Changed: {value}",
                    () => { Model.Name = value; });
                Notify();
            }
        }

        #endregion

    }
}