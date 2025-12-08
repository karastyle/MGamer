// <copyright file="VariableViewModel.cs" company="CarlosLab">
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
    public class VariableViewModel : BaseItemViewModel<Variable, BlackboardViewModel>, INameViewModel, IValueViewModel
        , INotifyBindablePropertyChanged
    {

        public event Action ValueChanged;

        [CreateProperty]
        public string Category
        {
            get => Model?.Category;
            set
            {
                if (Model == null || Model.Name == value) return;

                Record($"VariableViewModel Category Changed: {value}",
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
                if (Model == null || Model.Name == value)
                    return;

                Record($"VariableViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }

        [CreateProperty]
        public object ValueObject
        {
            get => Model?.ValueObject;
            set
            {
                if (Model == null || Model.ValueObject == value)
                    return;

                Record($"VariableViewModel Value Changed: {value}",
                    () => { Model.ValueObject = value; });

                OnValueChanged();
            }
        }

        public Type ValueType => Model.ValueType;

        public string TypeName => Model?.VariableType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;


        protected override void OnModelChanged(Variable newModel)
        {
            Notify(nameof(Name));
            NotifyValueChanged();
        }

        private void NotifyValueChanged()
        {
            Notify(nameof(ValueObject));
        }

        private void OnValueChanged()
        {
            NotifyValueChanged();
            ValueChanged?.Invoke();

            MakeDecision();
        }

        private void MakeDecision()
        {
            RootViewModel.MakeDecision();
        }
    }
}