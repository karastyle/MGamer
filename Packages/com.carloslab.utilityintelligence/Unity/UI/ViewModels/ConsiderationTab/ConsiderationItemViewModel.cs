// <copyright file="ConsiderationItemViewModel.cs" company="CarlosLab">
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
    public class ConsiderationItemViewModel : BaseItemViewModel<ConsiderationModel, ConsiderationListViewModel>,
        INameViewModel, INotifyBindablePropertyChanged
    {
        protected override void OnInit(ConsiderationModel model)
        {
            inputNormalizationViewModel = InputNormalizationListViewModel?.GetItemByName(model.InputNormalizationName);
            RegisterInputNormalizationEvents(inputNormalizationViewModel);
            EvaluateConsiderationScore();
        }

        protected override void OnDeinit()
        {
            UnregisterInputNormalizationEvents(inputNormalizationViewModel);
            inputNormalizationViewModel = null;
        }


        #region InputNormalization

        [CreateProperty]
        public float NormalizedInput => InputNormalizationViewModel?.NormalizedInput ?? 0;

        public event Action<InputNormalizationItemViewModel> InputNormalizationChanged;

        [CreateProperty]
        public string InputNormalizationName
        {
            get => Model?.InputNormalizationName ?? UtilityIntelligenceUIConsts.DefaultItemName;
            set
            {
                if (Model == null || Model.InputNormalizationName == value) return;

                Record($"ConsiderationItemViewModel InputNormalizationName Changed: {value}",
                    () => { Model.InputNormalizationName = value; });

                InputNormalizationViewModel = InputNormalizationListViewModel.GetItemByName(Model.InputNormalizationName);

                Notify();
            }
        }

        private InputNormalizationItemViewModel inputNormalizationViewModel;
        public InputNormalizationItemViewModel InputNormalizationViewModel
        {
            get => inputNormalizationViewModel;
            private set
            {
                if (inputNormalizationViewModel == value)
                    return;

                UnregisterInputNormalizationEvents(inputNormalizationViewModel);
                inputNormalizationViewModel = value;
                RegisterInputNormalizationEvents(inputNormalizationViewModel);

                OnInputNormalizationChanged(inputNormalizationViewModel);
            }
        }


        private void UnregisterInputNormalizationEvents(InputNormalizationItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged -= InputNormalizationViewModel_OnPropertyChanged;
            viewModel.InputChanged -= OnInputChanged;
        }

        private void RegisterInputNormalizationEvents(InputNormalizationItemViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.propertyChanged += InputNormalizationViewModel_OnPropertyChanged;
            viewModel.InputChanged += OnInputChanged;
        }

        private void InputNormalizationViewModel_OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(Name)) return;

            Notify(e.propertyName);
        }

        private void OnInputNormalizationChanged(InputNormalizationItemViewModel newNormalization)
        {
            EvaluateConsideration();

            OnInputChanged(newNormalization?.InputViewModel);
            InputNormalizationChanged?.Invoke(newNormalization);
        }

        #endregion

        #region InputNormalizationListViewModel

        public InputNormalizationListViewModel InputNormalizationListViewModel =>
            RootViewModel.InputNormalizationListViewModel;

        public event Action<InputNormalizationItemViewModel> InputNormalizationAdded;
        public event Action<InputNormalizationItemViewModel> InputNormalizationRemoved;
        public event Action<InputNormalizationItemViewModel, string, string> InputNormalizationNameChanged;

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.InputNormalizationListViewModel.ItemAdded += InputNormalizationListViewModel_OnItemAdded;
            viewModel.InputNormalizationListViewModel.ItemRemoved += InputNormalizationListViewModel_OnItemRemoved;
            viewModel.InputNormalizationListViewModel.ItemNameChanged += InputNormalizationListViewModel_OnItemNameChanged;
        }

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.InputNormalizationListViewModel.ItemAdded -= InputNormalizationListViewModel_OnItemAdded;
            viewModel.InputNormalizationListViewModel.ItemRemoved -= InputNormalizationListViewModel_OnItemRemoved;
            viewModel.InputNormalizationListViewModel.ItemNameChanged -= InputNormalizationListViewModel_OnItemNameChanged;
        }

        private void InputNormalizationListViewModel_OnItemAdded(InputNormalizationItemViewModel item)
        {
            InputNormalizationAdded?.Invoke(item);
        }

        private void InputNormalizationListViewModel_OnItemRemoved(InputNormalizationItemViewModel item)
        {
            if (InputNormalizationViewModel != item) return;

            InputNormalizationName = null;
            InputNormalizationRemoved?.Invoke(item);
        }

        private void InputNormalizationListViewModel_OnItemNameChanged(InputNormalizationItemViewModel item, string oldName, string newName)
        {
            if (InputNormalizationViewModel != item) return;

            InputNormalizationName = newName;
            InputNormalizationNameChanged?.Invoke(item, oldName, newName);
        }

        #endregion

        #region Input

        [CreateProperty] public string InputName => InputNormalizationViewModel?.InputName ?? UtilityIntelligenceUIConsts.DefaultItemName;

        public Type InputValueType => Model.InputValueType;

        [CreateProperty]
        public object RawInput
        {
            get => InputNormalizationViewModel?.RawInput;
            set
            {
                if (InputNormalizationViewModel != null)
                    InputNormalizationViewModel.RawInput = value;
            }
        }

        private InputItemViewModel inputViewModel;

        public InputItemViewModel InputViewModel => InputNormalizationViewModel?.InputViewModel;

        public event Action<InputItemViewModel> InputChanged;

        private void OnInputChanged(InputItemViewModel viewModel)
        {
            Notify(nameof(InputName));
            InputChanged?.Invoke(viewModel);
        }

        #endregion

        #region Consideration

        [CreateProperty]
        public string Category
        {
            get => Model?.Category;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"ConsiderationItemViewModel Category Changed: {value}",
                    () => { Model.Category = value; });

                Notify();
            }
        }

        [CreateProperty]
        public string Name
        {
            get => Model?.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"ConsiderationItemViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }

        [CreateProperty]
        public bool HasNoTarget
        {
            get => Model?.HasNoTarget ?? false;
            set
            {
                if (Model == null || Model.HasNoTarget == value) return;

                Record($"ConsiderationItemViewModel HasNoTarget Changed: {value}",
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

                Record($"ConsiderationItemViewModel EnableCachePerTarget Changed: {value}",
                    () => { Model.EnableCachePerTarget = value; });

                Notify();
            }
        }

        [CreateProperty]
        public InfluenceCurve ResponseCurve
        {
            get => Model.Runtime.ResponseCurve;
            set
            {
                Model.ResponseCurve = value;
                OnResponseCurveChanged();
                Notify();
            }
        }



        public void EvaluateConsideration()
        {
            if (IsRuntime) return;

            EvaluateConsiderationScore();
            MakeDecision();
        }

        private void EvaluateConsiderationScore()
        {
            if (IsRuntime || Model == null) return;

            var consideration = Model.Runtime;

            ConsiderationContext context = new(consideration);
            consideration.CalculateScore(ref context);
        }

        private void MakeDecision()
        {
            RootViewModel.MakeDecision();
        }

        private void OnResponseCurveChanged()
        {
            EvaluateConsideration();
        }

        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            if (newModel == null) return;

            Notify(nameof(Name));
            Notify(nameof(InputName));

            Notify(nameof(InputNormalizationName));

            Notify(nameof(HasNoTarget));
            Notify(nameof(EnableCachePerTarget));
            Notify(nameof(ResponseCurve));

            EvaluateConsiderationScore();
        }

        #endregion
    }
}