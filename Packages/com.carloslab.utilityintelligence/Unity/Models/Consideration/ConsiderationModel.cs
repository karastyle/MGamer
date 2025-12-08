// <copyright file="ConsiderationModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class ConsiderationModel : ContainerItemModel<ConsiderationContainerModel, Consideration>
    {
        public InputModel Input => InputNormalization?.Input;

        #region InputNormalization

        private InputNormalizationContainerModel inputNormalizationContainer;

        public InputNormalizationContainerModel InputNormalizationContainer
        {
            get => inputNormalizationContainer;
            internal set => inputNormalizationContainer = value;
        }

        [DataMember(Name = nameof(InputNormalization))]
        private string inputNormalizationName;

        public string InputNormalizationName
        {
            get => inputNormalizationName;
            set
            {
                if (inputNormalizationName == value) return;

                inputNormalizationName = value;
                if (inputNormalizationContainer != null
                    && inputNormalizationContainer.TryGetItem(inputNormalizationName,
                        out InputNormalizationModel inputNormalization))
                    InputNormalization = inputNormalization;
            }
        }

        private InputNormalizationModel inputNormalization;

        public InputNormalizationModel InputNormalization
        {
            get
            {

                if (inputNormalization == null)
                {
                    if (!string.IsNullOrEmpty(inputNormalizationName)
                        && inputNormalizationContainer != null)
                    {
                        if (inputNormalizationContainer.TryGetItem(inputNormalizationName, out InputNormalizationModel inputNormalization))
                            this.inputNormalization = inputNormalization;
                        else
                            StaticConsole.LogWarning($"Asset: {Asset?.Name} Consideration: {Name} Cannot find the NormalizedInput: {inputNormalizationName}. Please remove it from your JSON using File Menu Toolbar.");
                    }
                }

                return inputNormalization;
            }
            set
            {
                if (inputNormalization == value) return;

                inputNormalization = value;
                Runtime.InputNormalization = inputNormalization?.Runtime;
            }
        }

        #endregion

        #region Consideration

        [DataMember(Name = nameof(Category))]
        private string category;

        public string Category
        {
            get => category;
            internal set
            {
                category = value;
                Runtime.Category = value;
            }
        }

        [DataMember(Name = nameof(HasNoTarget))]
        private bool hasNoTarget;
        public bool HasNoTarget
        {
            get => hasNoTarget;
            set
            {
                hasNoTarget = value;
                Runtime.HasNoTarget = value;
            }
        }

        [DataMember(Name = nameof(EnableCachePerTarget))]
        private bool enableCachePerTarget;
        public bool EnableCachePerTarget
        {
            get => enableCachePerTarget;
            set
            {
                enableCachePerTarget = value;
                Runtime.EnableCachePerTarget = value;
            }
        }

        [DataMember(Name = nameof(ResponseCurve))]
        private InfluenceCurve responseCurve = InfluenceCurve.BasicLinear;

        public InfluenceCurve ResponseCurve
        {
            get => responseCurve;
            set
            {
                responseCurve = value;
                Runtime.ResponseCurve = value;
            }
        }

        public Type InputValueType => inputNormalization?.InputValueType;

        #endregion

        #region Runtime

        private Consideration runtime;

        public override Consideration Runtime
        {
            get
            {
                InputNormalization inputNormalization = InputNormalization?.Runtime;
                if (runtime == null)
                {
                    runtime = new Consideration(inputNormalization, responseCurve)
                    {
                        HasNoTarget = hasNoTarget,
                        EnableCachePerTarget = enableCachePerTarget
                    };
                }

                return runtime;
            }
        }

        #endregion
    }
}