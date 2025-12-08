// <copyright file="ConsiderationContextViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Extensions;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationContextViewModel : UtilityIntelligenceViewModelMember<ConsiderationModel>, IScoreViewModel, ITargetViewModel
        , IDataSourceViewHashProvider
    {
        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            this.context = ConsiderationContext.Null;
            UpdateViewHashCode();
        }

        private ConsiderationItemViewModel considerationViewModel;

        public ConsiderationItemViewModel ConsiderationViewModel
        {
            get => considerationViewModel;
            internal set => considerationViewModel = value;
        }

        #region ConsiderationContext

        private ConsiderationContext context;

        private void OnContextChanged(DecisionContext context)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var considerationContext = context.GetConsiderationContext(Model.Name);
            this.context = considerationContext;
            UpdateViewHashCode();

            Notify(nameof(CurrentStatus));
#endif
        }

        #endregion

        #region DecisionContextViewModel

        private DecisionContextViewModel decisionContextViewModel;

        public DecisionContextViewModel DecisionContextViewModel
        {
            get => decisionContextViewModel;
            internal set
            {
                if (decisionContextViewModel == value)
                    return;

                if (decisionContextViewModel != null)
                    UnregisterDecisionContextViewModelEvents(decisionContextViewModel);

                decisionContextViewModel = value;

                if (decisionContextViewModel != null)
                    RegisterDecisionContextViewModelEvents(decisionContextViewModel);
            }
        }

        private void RegisterDecisionContextViewModelEvents(DecisionContextViewModel viewModel)
        {
            OnContextChanged(viewModel.Context);
            viewModel.ContextChanged += OnContextChanged;
        }

        private void UnregisterDecisionContextViewModelEvents(DecisionContextViewModel viewModel)
        {
            viewModel.ContextChanged -= OnContextChanged;
        }

        #endregion

        #region Input

        public Type InputValueType => Model.Runtime.InputValueType;

        public object DefaultInputValue => InputValueType?.GetDefaultValue();

        [CreateProperty]
        public object RawInput
        {
            get
            {
                if (context != ConsiderationContext.Null)
                {
                    switch (context.CurrentStatus)
                    {
                        case ConsiderationStatus.Executed:
                            if (context.RawInput != null)
                                return context.RawInput;
                            break;
                        case ConsiderationStatus.Discarded:
                            if (!IsRuntime)
                                return context.Consideration.RawInput;
                            break;
                    }
                }

                return DefaultInputValue;
            }
            set => considerationViewModel.RawInput = value;
        }

        #endregion

        #region Properties

        [CreateProperty]
        public float NormalizedInput
        {
            get
            {
                if (context != ConsiderationContext.Null)
                    return context.NormalizedInput;

                return 0.0f;
            }
        }

        [CreateProperty]
        public float Score
        {
            get
            {
                if (context != ConsiderationContext.Null)
                {
                    return context.FinalScore;
                }

                var responseCurve = ResponseCurve;
                return InfluenceCurveUtils.Evaluate(0.0f, in responseCurve);
            }
        }

        [CreateProperty] public string TargetName => context.Target?.Name ?? "None";

        [CreateProperty] public InfluenceCurve ResponseCurve => Model.Runtime.ResponseCurve;

        public ConsiderationStatus CurrentStatus
        {
            get
            {
                if (context != ConsiderationContext.Null)
                    return context.CurrentStatus;
                else
                    return ConsiderationStatus.Discarded;
            }
        }


        #endregion
    }
}