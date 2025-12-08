// <copyright file="InputNormalizationItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationItemViewModel : BaseItemViewModel<InputNormalizationModel, InputNormalizationListViewModel>, ITypeNameViewModel, INameViewModel
        , INotifyBindablePropertyChanged

    {
        protected override void OnInit(InputNormalizationModel model)
        {
            inputViewModel = InputListViewModel?.GetItemByName(model.InputName);
            RegisterInputEvents(inputViewModel);
        }

        protected override void OnDeinit()
        {
            UnregisterInputEvents(inputViewModel);
            inputViewModel = null;
        }

        protected override void OnItemAdded()
        {
            EvaluateNormalizedInput();
        }

        #region Input

        public string InputName
        {
            get => Model?.InputName ?? UtilityIntelligenceUIConsts.DefaultItemName;
            set
            {
                if (Model == null || Model.InputName == value)
                    return;

                Record($"InputNormalizationItemViewModel InputName Changed: {value}",
                    () => { Model.InputName = value; });

                InputViewModel = InputListViewModel.GetItemByName(Model.InputName);
            }
        }

        [CreateProperty]
        public object RawInput
        {
            get => InputViewModel?.ValueObject;
            set
            {
                if (InputViewModel != null)
                    InputViewModel.ValueObject = value;
            }
        }

        public Type InputType => Model?.InputType;
        public Type InputValueType => Model?.InputValueType;

        public event Action<InputItemViewModel> InputChanged;


        private InputItemViewModel inputViewModel;

        public InputItemViewModel InputViewModel
        {
            get => inputViewModel;
            private set
            {
                if (inputViewModel == value)
                    return;

                UnregisterInputEvents(inputViewModel);

                inputViewModel = value;

                RegisterInputEvents(inputViewModel);

                OnInputChanged(inputViewModel);
            }
        }

        private void RegisterInputEvents(InputItemViewModel input)
        {
            if (input == null) return;

            input.ValueChanged += InputViewModel_OnValueChanged;
        }

        private void UnregisterInputEvents(InputItemViewModel input)
        {
            if (input == null) return;

            input.ValueChanged -= InputViewModel_OnValueChanged;
        }

        private void InputViewModel_OnValueChanged()
        {
            EvaluateInputNormalization();
            Notify(nameof(RawInput));
        }

        private void OnInputChanged(InputItemViewModel newInput)
        {
            EvaluateInputNormalization();
            InputChanged?.Invoke(newInput);
        }

        #endregion

        #region RootViewModel

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.InputListViewModel.ItemAdded += InputListViewModel_OnItemAdded;
            viewModel.InputListViewModel.ItemRemoved += InputListViewModel_OnItemRemoved;
            viewModel.InputListViewModel.ItemNameChanged += InputListViewModel_OnItemNameChanged;

            viewModel.BlackboardViewModel.ItemRemoved += BlackboardViewModel_OnItemRemoved;
            viewModel.BlackboardViewModel.ItemNameChanged += BlackboardViewModel_OnItemNameChanged;
        }

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.InputListViewModel.ItemAdded -= InputListViewModel_OnItemAdded;
            viewModel.InputListViewModel.ItemRemoved -= InputListViewModel_OnItemRemoved;
            viewModel.InputListViewModel.ItemNameChanged -= InputListViewModel_OnItemNameChanged;

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

        #region InputListViewModel

        public event Action<InputItemViewModel> InputAdded;
        public event Action<InputItemViewModel> InputRemoved;
        public event Action<InputItemViewModel, string, string> InputNameChanged;


        public InputListViewModel InputListViewModel => RootViewModel.InputListViewModel;



        private void InputListViewModel_OnItemAdded(InputItemViewModel item)
        {
            InputAdded?.Invoke(item);
        }

        private void InputListViewModel_OnItemRemoved(InputItemViewModel item)
        {
            if (InputViewModel != item) return;

            InputName = null;
            InputRemoved?.Invoke(item);
        }

        private void InputListViewModel_OnItemNameChanged(InputItemViewModel item, string oldName, string newName)
        {
            if (InputViewModel != item) return;

            InputName = newName;
            InputNameChanged?.Invoke(item, oldName, newName);
        }

        #endregion

        #region InputNormalization

        [CreateProperty]
        public string Name
        {
            get => Model?.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"InputNormalizationItemViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }

        [CreateProperty]
        public string Category
        {
            get => Model?.Category;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"InputNormalizationItemViewModel Category Changed: {value}",
                    () => { Model.Category = value; });

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

                Record($"InputNormalizationItemViewModel HasNoTarget Changed: {value}",
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

                Record($"InputNormalizationItemViewModel EnableCachePerTarget Changed: {value}",
                    () => { Model.EnableCachePerTarget = value; });

                Notify();
            }
        }

        [CreateProperty]
        public float NormalizedInput => Model?.Runtime.NormalizedInput ?? 0;

        public Type Type => Model?.RuntimeType;
        public string TypeName => Model?.RuntimeType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;


        private void EvaluateNormalizedInput()
        {
            if (IsRuntime || Model == null) return;

            InputNormalizationContext context = new(Model.Runtime);
            Model.Runtime.CalculateNormalizedInput(in context);
        }

        public void EvaluateInputNormalization()
        {
            if (IsRuntime) return;

            EvaluateNormalizedInput();
            MakeDecision();

            OnNormalizedInputChanged();
        }

        private void MakeDecision()
        {
            RootViewModel.MakeDecision();
        }

        private void OnNormalizedInputChanged()
        {
            Notify(nameof(NormalizedInput));
        }

        #endregion

        #region Model

        protected override void OnModelChanged(InputNormalizationModel newModel)
        {
            if (newModel == null) return;

            Notify(nameof(Name));
            Notify(nameof(InputName));
            Notify(nameof(HasNoTarget));
            Notify(nameof(EnableCachePerTarget));

            EvaluateNormalizedInput();
        }

        #endregion
    }
}