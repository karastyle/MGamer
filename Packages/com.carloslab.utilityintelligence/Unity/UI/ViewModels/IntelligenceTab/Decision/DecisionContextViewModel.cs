// <copyright file="DecisionContextViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionContextViewModel : UtilityIntelligenceViewModelMember<DecisionModel>, IScoreViewModel, ITargetViewModel
        , IDataSourceViewHashProvider
    {
        #region DecisionMakerContextViewModel

        private DecisionMakerContextViewModel decisionMakerContextViewModel;

        public DecisionMakerContextViewModel DecisionMakerContextViewModel
        {
            get => decisionMakerContextViewModel;
            internal set
            {
                if (decisionMakerContextViewModel == value)
                    return;

                UnregisterDecisionMakerViewModelEvents(decisionMakerContextViewModel);

                decisionMakerContextViewModel = value;

                RegisterDecisionMakerViewModelEvents(decisionMakerContextViewModel);
            }
        }

        private void RegisterDecisionMakerViewModelEvents(DecisionMakerContextViewModel viewModel)
        {
            if (viewModel == null) return;

            OnContextChanged(viewModel.Context);
            viewModel.ContextChanged += OnContextChanged;
        }

        private void UnregisterDecisionMakerViewModelEvents(DecisionMakerContextViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.ContextChanged -= OnContextChanged;
        }

        #endregion

        #region DecisionContext

        private DecisionContext context;

        public DecisionContext Context => context;

        public bool IsMatchContext
        {
            get
            {
                var decision = Model.Runtime;
                var decisionMaker = decisionMakerContextViewModel.Model.Runtime;
                var context = Model.Runtime.Context;

                if (context.DecisionMaker == decisionMaker
                    && context.Decision == decision)
                    return true;

                return false;
            }
        }

        public event Action<DecisionContext> ContextChanged;


        private void OnContextChanged(DecisionMakerContext decisionMakerContext)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var decisionContext = decisionMakerContext.GetDecisionContext(Model.Name);

            this.context = decisionContext;
            ContextChanged?.Invoke(decisionContext);
            UpdateViewHashCode();
#endif
        }

        protected override void OnModelChanged(DecisionModel newModel)
        {
            context = DecisionContext.Null;
            UpdateViewHashCode();
        }

        #endregion

        #region Properties

        [CreateProperty]
        public string TargetName => context.Target?.Name ?? "None";

        [CreateProperty]
        public float Score
        {
            get
            {
                if (context != DecisionContext.Null)
                    return context.FinalScore;
                else
                    return 0.0f;
            }
        }

        #endregion
    }
}