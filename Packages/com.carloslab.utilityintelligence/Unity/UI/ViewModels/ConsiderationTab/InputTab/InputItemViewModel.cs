// <copyright file="InputItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputItemViewModel : BaseItemViewModel<InputModel, InputListViewModel>, ITypeNameViewModel, INameViewModel, IValueViewModel
        , INotifyBindablePropertyChanged

    {

        #region Input

        public string TypeName => Model?.RuntimeType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;

        [CreateProperty]
        public string Category
        {
            get => Model?.Category;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"InputItemViewModel Category Changed: {value}",
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

                Record($"InputItemViewModel Name Changed: {value}",
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

                Record($"InputItemViewModel HasNoTarget Changed: {value}",
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

                Record($"InputItemViewModel EnableCachePerTarget Changed: {value}",
                    () => { Model.EnableCachePerTarget = value; });

                Notify();
            }
        }

        #endregion

        #region Value

        public Type ValueType => Model.ValueType;

        public event Action ValueChanged;

        private object valueObject;

        [CreateProperty]
        public object ValueObject
        {
            get => Model.Runtime.ValueObject;
            set
            {
                if (Model.Runtime.ValueObject == value) return;

                Model.Runtime.ValueObject = value;
                valueObject = value;
                OnValueChanged();
            }
        }

        private void NotifyValueChanged()
        {
            Notify(nameof(ValueObject));
        }

        private void OnValueChanged()
        {
            NotifyValueChanged();
            ValueChanged?.Invoke();
        }

        #endregion

        #region Model

        protected override void OnModelChanged(InputModel newModel)
        {
            if (newModel == null) return;

            Notify(nameof(Name));
            Notify(nameof(HasNoTarget));
            Notify(nameof(EnableCachePerTarget));

            if (RootViewModel.Asset.IsInUndoRedo)
            {
                ValueObject = valueObject;
                NotifyValueChanged();
            }
        }

        protected override void OnRegisterModelEvents(InputModel model)
        {
            model.Runtime.ValueChanged += NotifyValueChanged;
        }

        protected override void OnUnregisterModelEvents(InputModel model)
        {
            model.Runtime.ValueChanged -= NotifyValueChanged;
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