// <copyright file="InputNormalizationModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;
using System;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<InputNormalizationModel>))]
    public class InputNormalizationModel : GenericModelItem<InputNormalizationContainerModel, InputNormalization>
    {
        public string Category
        {
            get => (string)GetValue(nameof(Category));
            internal set => SetValue(nameof(Category), value);
        }

        public bool HasNoTarget
        {
            get => (bool)GetValue(nameof(HasNoTarget));
            internal set => SetValue(nameof(HasNoTarget), value);
        }

        public bool EnableCachePerTarget
        {
            get => (bool)GetValue(nameof(EnableCachePerTarget));
            internal set => SetValue(nameof(EnableCachePerTarget), value);
        }

        #region Input

        private InputContainerModel inputContainer;

        public InputContainerModel InputContainer
        {
            get => inputContainer;
            internal set => inputContainer = value;
        }

        public string InputName
        {
            get => GetValue(nameof(Input)) as string;
            internal set
            {
                string inputName = value;
                SetValue(nameof(Input), inputName);

                if (inputContainer != null
                    && inputContainer.TryGetItem(inputName, out InputModel input))
                    Input = input;
            }
        }

        private InputModel input;
        public InputModel Input
        {
            get
            {
                if (input == null)
                {
                    string inputName = InputName;

                    if (!string.IsNullOrEmpty(inputName)
                        && inputContainer != null)
                    {
                        if (inputContainer.TryGetItem(inputName, out InputModel input))
                            this.input = input;
                        else
                            StaticConsole.LogWarning($"Asset: {Asset?.Name} NormalizedInput: {Name} Cannot find the RawInput: {inputName}." +
                                                     $" Please remove it from your JSON using File Menu Toolbar.");
                    }
                }

                return input;
            }
            set
            {
                if (input == value) return;

                input = value;
                Runtime.Input = value.Runtime;
            }
        }

        public Type InputType => Runtime.InputType;
        public Type InputValueType => Runtime.InputValueType;

        #endregion

        protected override InputNormalization CreateRuntimeObject()
        {
            var runtime = base.CreateRuntimeObject();
            runtime.Input = Input?.Runtime;
            return runtime;
        }
    }
}