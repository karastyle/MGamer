// <copyright file="UtilityIntelligence.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class UtilityIntelligence : BaseStateMachine<DecisionMaker>, IUtilityIntelligence
    {
        #region Init

        public UtilityIntelligence()
        {
            blackboard = new();

            inputContainer = new();
            inputContainer.RootObject = this;

            inputNormalizationContainer = new();
            inputNormalizationContainer.RootObject = this;

            considerationContainer = new();
            considerationContainer.RootObject = this;

            targetFilterContainer = new();
            targetFilterContainer.RootObject = this;

            decisionContainer = new();
            decisionContainer.RootObject = this;

            decisionMakerContainer = new();
            decisionMakerContainer.RootObject = this;
        }

        public UtilityIntelligence(DecisionMakerContainer decisionMakers, DecisionContainer decisions, TargetFilterContainer targetFilters, ConsiderationContainer considerations,
            InputContainer inputs, InputNormalizationContainer inputNormalizations, Blackboard blackboard)
        {
            this.blackboard = blackboard ?? new();

            inputContainer = inputs ?? new();
            inputContainer.RootObject = this;

            inputNormalizationContainer = inputNormalizations ?? new();
            inputNormalizationContainer.RootObject = this;

            considerationContainer = considerations ?? new();
            considerationContainer.RootObject = this;

            targetFilterContainer = targetFilters ?? new();
            targetFilterContainer.RootObject = this;

            decisionContainer = decisions ?? new();
            decisionContainer.RootObject = this;

            decisionMakerContainer = decisionMakers ?? new();
            decisionMakerContainer.RootObject = this;
        }

        #endregion

        #region Properties

        private bool isRuntimeEditorOpening;

        public bool IsRuntimeEditorOpening
        {
            get => IsEditorOpening && isRuntimeEditorOpening;
            internal set => isRuntimeEditorOpening = value;
        }

        private int editorOpeningCount;
        public int EditorOpeningCount
        {
            get => editorOpeningCount;
            internal set => editorOpeningCount = value;
        }
        public bool IsEditorOpening => editorOpeningCount > 0;

        public override bool IsRuntime { get; internal set; }

        private bool enableCompensationFactor;
        public bool EnableCompensationFactor
        {
            get => enableCompensationFactor;
            internal set => enableCompensationFactor = value;
        }

        private float momentumBonus;

        public float MomentumBonus
        {
            get => momentumBonus;
            internal set => momentumBonus = value;
        }

        public Decision CurrentDecision => state?.State;

        private DecisionMakerContainer decisionMakerContainer;
        public DecisionMakerContainer DecisionMakerContainer => decisionMakerContainer;


        private DecisionContainer decisionContainer;
        public DecisionContainer DecisionContainer => decisionContainer;


        private TargetFilterContainer targetFilterContainer;
        public TargetFilterContainer TargetFilterContainer => targetFilterContainer;


        private ConsiderationContainer considerationContainer;
        public ConsiderationContainer ConsiderationContainer => considerationContainer;


        private InputNormalizationContainer inputNormalizationContainer;
        public InputNormalizationContainer InputNormalizationContainer => inputNormalizationContainer;

        private InputContainer inputContainer;
        public InputContainer InputContainer => inputContainer;


        private Blackboard blackboard;
        public Blackboard Blackboard => blackboard;

        #endregion

        #region Agent

        private UtilityAgent agent;

        public UtilityAgent Agent
        {
            get => agent;
            internal set => agent = value;
        }
        public IEntityFacade AgentFacade => agent.EntityFacade;

        public override string AgentName => agent?.Name;

        public T GetComponent<T>()
        {
            return agent.GetComponent<T>();
        }

        public T GetComponentInChildren<T>()
        {
            return agent.GetComponentInChildren<T>();
        }

        #endregion

        #region Contexts

        public event Action<UtilityIntelligenceContext> ContextChanged;

        private UtilityIntelligenceContext context;

        public UtilityIntelligenceContext Context
        {
            get => context;
            private set
            {
                context = value;
                // UtilityIntelligenceConsole.Instance.Log($"Agent: {AgentName} UtilityIntelligence ContextChanged IsEditorOpening: {IsEditorOpening}");

                ContextChanged?.Invoke(context);
            }
        }

        #endregion

        #region Make Decision

        private void OnBeforeMakeDecision(HashSet<UtilityEntity> entities)
        {
            // #if CARLOSLAB_ENABLE_PROFILER
            //             using var _ = Profiler.Sample("UtilityIntelligence - OnBeforeMakeDecision");
            // #endif
            FilterTargets(entities);
        }

        private void OnAfterMakeDecision()
        {
            // #if CARLOSLAB_ENABLE_PROFILER
            //             using var _ = Profiler.Sample("UtilityIntelligence - OnAfterMakeDecision");
            // #endif
            ResetTargetFilters();
            ResetInputs();
            ResetInputNormalizations();
            ResetConsiderations();
            ResetDecisions();
        }

        internal void MakeDecision(HashSet<UtilityEntity> entities)
        {
            if (!CanGoToNextState) return;

            //#if CARLOSLAB_ENABLE_PROFILER
            //            using var _ = Profiler.Sample("UtilityIntelligence - MakeDecision");
            //#endif
            OnBeforeMakeDecision(entities);

            UtilityIntelligenceContext intelligenceContext = new(this);
            MakeDecision(entities, ref intelligenceContext);

            OnAfterMakeDecision();

            Context = intelligenceContext;
        }

        private void MakeDecision(HashSet<UtilityEntity> entities, ref UtilityIntelligenceContext intelligenceContext)
        {
            DecisionMakerContext bestDecisionMakerContext = DecisionMakerContext.Null;
            DecisionContext bestDecisionContext = DecisionContext.Null;

            DecisionMaker bestDecisionMaker = null;
            Decision bestDecision = null;

            float bestDecisionMakerScore = 0.0f;

            if (decisionMakerContainer.Count > 0)
            {
                var decisionMakers = decisionMakerContainer.Items;
                for (int index = 0; index < decisionMakers.Count; index++)
                {
                    DecisionMaker decisionMaker = decisionMakers[index];
                    DecisionMakerContext decisionMakerContext = new(decisionMaker);
                    DecisionContext decisionContext =
                        decisionMaker.MakeDecision(entities, bestDecisionMakerScore, ref decisionMakerContext);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    intelligenceContext.AddDecisionMakerContext(decisionMaker.Name, in decisionMakerContext);
#endif

                    float decisionMakerScore = decisionMakerContext.Score;

                    if (decisionMakerScore > bestDecisionMakerScore)
                    {
                        bestDecisionMakerScore = decisionMakerScore;
                        bestDecisionMakerContext = decisionMakerContext;
                        bestDecisionContext = decisionContext;
                    }

                    if (bestDecisionMakerContext == DecisionMakerContext.Null)
                    {
                        bestDecisionMakerContext = decisionMakerContext;
                        bestDecisionContext = decisionContext;
                    }
                }

            }

            bestDecisionMaker = bestDecisionMakerContext.DecisionMaker;
            if (bestDecisionMaker != null)
            {
                intelligenceContext.BestDecisionMaker = bestDecisionMaker;
                intelligenceContext.Score = bestDecisionMakerContext.Score;
                bestDecisionMakerContext.IsWinner = true;

                bestDecision = bestDecisionContext.Decision;
                if (bestDecision != null)
                {
                    intelligenceContext.BestDecision = bestDecision;
                    bestDecisionContext.IsWinner = true;

                    bestDecision.NextContext = bestDecisionContext;

                    StateMachineConsole.Instance.Log($"Agent: {AgentName} MakeDecision BestDecision: {bestDecision?.Name} Score : {bestDecisionContext.FinalScore} Frame: {WorldTime.Instance.Frame}");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    bestDecisionMakerContext.SetDecisionContext(bestDecision.Name, in bestDecisionContext);
#endif
                }

                bestDecisionMaker.NextContext = bestDecisionMakerContext;
                bestDecisionMaker.NextState = bestDecision;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                intelligenceContext.SetDecisionMakerContext(bestDecisionMaker.Name, in bestDecisionMakerContext);
#endif


            }

            NextState = bestDecisionMaker;

            //UtilityIntelligenceConsole.Instance.Log($"Agent: {AgentName} MakeDecision End BestDecision: {bestDecision?.Name} BestDecisionMaker: {bestDecisionMaker?.Name}");
        }

        #endregion

        #region Target Filters

        private void ResetTargetFilters()
        {
            var items = targetFilterContainer.Items;
            for (int index = 0; index < items.Count; index++)
            {
                TargetFilter item = items[index];
                item.Reset();
            }
        }

        private void FilterTargets(HashSet<UtilityEntity> entities)
        {
            // #if CARLOSLAB_ENABLE_PROFILER
            //             using var _ = Profiler.Sample("MakeDecision - FilterTargets");
            // #endif
            if (entities == null || entities.Count == 0) return;

            var items = targetFilterContainer.Items;
            foreach (var entity in entities)
            {
                for (int index = 0; index < items.Count; index++)
                {
                    TargetFilter item = items[index];
                    item.FilterTarget(entity);
                }
            }
        }

        #endregion

        #region Decisions

        private void ResetDecisions()
        {
            var items = decisionContainer.Items;
            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                item.Reset();
            }
        }

        #endregion

        #region Inputs

        private void ResetInputs()
        {
            var items = inputContainer.Items;
            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                item.Reset();
            }
        }

        #endregion

        #region InputNormalization

        private void ResetInputNormalizations()
        {
            var items = inputNormalizationContainer.Items;
            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                item.Reset();
            }
        }

        #endregion

        #region Considerations

        private void ResetConsiderations()
        {
            var items = considerationContainer.Items;
            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                item.Reset();
            }
        }

        internal float CalculateGeometricMean(float[] considerationScores)
        {
            if (considerationScores.Length == 0)
            {
                throw new ArgumentException("Array of scores must not be empty.");
            }
            float product = 1.0f;

            foreach (float considerationScore in considerationScores)
            {
                product *= considerationScore;
            }

            float geometricMean = MathF.Pow(product, 1.0f / considerationScores.Length);

            return geometricMean;
        }

        #endregion

        #region Events

        public event Action<Decision> DecisionChanged;

        protected override void OnRegisterStateEvents(DecisionMaker decisionMaker)
        {
            decisionMaker.StateChanged += OnDecisionChanged;
        }

        protected override void OnUnregisterStateEvents(DecisionMaker decisionMaker)
        {
            decisionMaker.StateChanged -= OnDecisionChanged;
        }

        private void OnDecisionChanged(Decision decision)
        {
            DecisionChanged?.Invoke(decision);
        }

        #endregion
    }
}