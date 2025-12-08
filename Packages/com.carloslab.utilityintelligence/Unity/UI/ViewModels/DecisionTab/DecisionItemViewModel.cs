// <copyright file="DecisionItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionItemViewModel : BaseItemViewModel<DecisionModel, DecisionListViewModel>,
        INameViewModel, INotifyBindablePropertyChanged
    {
        [CreateProperty]
        public string Category
        {
            get => Model?.Category;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"DecisionItemViewModel Category Changed: {value}",
                    () => { Model.Category = value; });

                Notify();
            }
        }

        protected override void OnInit(DecisionModel model)
        {
            actionListViewModel.Init(model);
            targetFilterListViewModel.Init(model);
            considerationListViewModel.Init(model);
        }

        protected override void OnDeinit()
        {
            actionListViewModel.Deinit();
            targetFilterListViewModel.Deinit();
            considerationListViewModel.Deinit();
        }

        protected override void OnRootViewModelChanged(UtilityIntelligenceViewModel rootViewModel)
        {
            actionListViewModel.RootViewModel = rootViewModel;
            targetFilterListViewModel.RootViewModel = rootViewModel;
            considerationListViewModel.RootViewModel = rootViewModel;
        }

        #region ViewModels

        private ActionListViewModel actionListViewModel = new();
        public ActionListViewModel ActionListViewModel => actionListViewModel;

        private TargetFilterListViewModelDecisionTab targetFilterListViewModel = new();

        public TargetFilterListViewModelDecisionTab TargetFilterListViewModel => targetFilterListViewModel;


        private ConsiderationListViewModelDecisionTab considerationListViewModel = new();

        public ConsiderationListViewModelDecisionTab ConsiderationListViewModel => considerationListViewModel;

        #endregion

        #region Actions

        [CreateProperty]
        public ActionExecutionMode ActionExecutionMode
        {
            get => Model.ActionExecutionMode;
            set
            {
                if (Model.ActionExecutionMode == value)
                    return;

                Record($"DecisionViewModel ActionsExecutionMode Changed: {value}",
                    () =>
                    {
                        Model.SetActionsExecutionMode(value, IsRuntime);
                    });

                Notify();
            }
        }

        [CreateProperty]
        public bool KeepRunningUntilFinished
        {
            get => Model.KeepRunningUntilFinished;
            set
            {
                if (Model.KeepRunningUntilFinished == value) return;

                Record($"DecisionViewModel KeepRunningUntilFinished Changed: {value}",
                    () => { Model.KeepRunningUntilFinished = value; });
            }
        }

        [CreateProperty]
        public int MaxRepeatCount
        {
            get => Model.MaxRepeatCount;
            set
            {
                if (Model.MaxRepeatCount == value) return;

                Record($"DecisionViewModel MaxRepeatCount Changed: {value}",
                    () => { Model.MaxRepeatCount = value; });
            }
        }


        #endregion

        #region Targets

        [CreateProperty]
        public bool HasNoTarget
        {
            get => Model.HasNoTarget;
            set
            {
                if (Model.HasNoTarget == value) return;

                Record($"DecisionViewModel HasNoTarget Changed: {value}",
                    () => { Model.HasNoTarget = value; });
                Notify();
            }
        }

        [CreateProperty]
        public bool EnableCachePerTarget
        {
            get => Model?.EnableCachePerTarget ?? false;
            set
            {
                if (Model == null || Model.EnableCachePerTarget == value) return;

                Record($"DecisionViewModel EnableCachePerTarget Changed: {value}",
                    () => { Model.EnableCachePerTarget = value; });

                Notify();
            }
        }


        #endregion

        #region Decision

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value)
                    return;

                Record($"DecisionViewModel Name Changed: {value}",
                    () => { Model.Name = value; });
                Notify();
            }
        }

        [CreateProperty]
        public float Weight
        {
            get => Model.Weight;
            set
            {
                if (Math.Abs(Model.Weight - value) < MathUtils.Epsilon)
                    return;

                Record($"DecisionItemViewModel Weight Changed: {value}",
                    () => { Model.Weight = value; });

                Notify();
            }
        }

        #endregion

        #region Model

        protected override void OnModelChanged(DecisionModel newModel)
        {
            ActionListViewModel.Model = newModel;
            TargetFilterListViewModel.Model = newModel;
            ConsiderationListViewModel.Model = newModel;

            if (newModel == null) return;

            Notify(nameof(Name));
            Notify(nameof(Weight));
            Notify(nameof(ActionExecutionMode));
            Notify(nameof(HasNoTarget));
            Notify(nameof(EnableCachePerTarget));

            Notify(nameof(KeepRunningUntilFinished));
            Notify(nameof(MaxRepeatCount));
        }

        #endregion

        #region RootViewModel

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.BlackboardViewModel.ItemRemoved += BlackboardViewModel_OnItemRemoved;
            viewModel.BlackboardViewModel.ItemNameChanged += BlackboardViewModel_OnItemNameChanged;
        }

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.BlackboardViewModel.ItemRemoved -= BlackboardViewModel_OnItemRemoved;
            viewModel.BlackboardViewModel.ItemNameChanged -= BlackboardViewModel_OnItemNameChanged;

        }

        #endregion

        #region BlackboardViewModel

        private void BlackboardViewModel_OnItemRemoved(VariableViewModel variable)
        {
            Model.SetVariableReference(variable.Name, null);
        }

        private void BlackboardViewModel_OnItemNameChanged(VariableViewModel variable, string oldName, string newName)
        {
            Model.SetVariableReference(oldName, newName);
        }

        #endregion
    }
}