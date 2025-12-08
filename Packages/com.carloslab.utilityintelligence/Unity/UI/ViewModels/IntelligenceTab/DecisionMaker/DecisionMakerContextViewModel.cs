// <copyright file="DecisionMakerContextViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerContextViewModel : UtilityIntelligenceViewModelMember<DecisionMakerModel>, IScoreViewModel, IDataSourceViewHashProvider
    {
        protected override void OnInit(DecisionMakerModel model)
        {
            OnContextChanged(RootViewModel.ContextViewModel.Context);
        }

        #region RootViewModel Events

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.ContextViewModel.ContextChanged += OnContextChanged;
        }

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            var contextViewModel = viewModel.ContextViewModel;
            contextViewModel.ContextChanged -= OnContextChanged;
        }

        #endregion

        #region DecisionMakerContext

        private DecisionMakerContext context;
        public DecisionMakerContext Context => context;

        public event Action<DecisionMakerContext> ContextChanged;


        private void OnContextChanged(UtilityIntelligenceContext context)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var decisionMakerContext = context.GetDecisionMakerContext(Model.Name);

            // Debug.Log($"MatchDecisionMakerContext OnContextChanged Name: {Model.Runtime.Intelligence?.Name} IsEditorOpening: {Model.Runtime.Intelligence?.IsEditorOpening} decisionMaker == null: {decisionMakerContext.DecisionMaker == null}");

            this.context = decisionMakerContext;
            ContextChanged?.Invoke(decisionMakerContext);
            UpdateViewHashCode();
#endif
        }
        protected override void OnModelChanged(DecisionMakerModel newModel)
        {
            this.context = DecisionMakerContext.Null;
            UpdateViewHashCode();
        }

        #endregion

        #region Properties

        [CreateProperty]
        public float Score
        {
            get
            {
                if (context != DecisionMakerContext.Null)
                    return context.Score;
                else
                    return 0.0f;
            }
        }

        [CreateProperty] public string BestDecisionName => context.BestDecisionName;

        #endregion
    }
}