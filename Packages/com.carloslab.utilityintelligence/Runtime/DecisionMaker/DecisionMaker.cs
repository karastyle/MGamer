// <copyright file="DecisionMaker.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Extensions;
using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class DecisionMaker : UtilityIntelligenceMemberStateMachineState<Decision>, IContainerItem
    {
        public override string AgentName => Intelligence.AgentName;

        #region Decisions

        private readonly Dictionary<string, Decision> decisionDict = new();

        private readonly List<Decision> decisions = new();
        public IReadOnlyList<Decision> Decisions => decisions;


        public bool HasDecision(string name)
        {
            return decisionDict.ContainsKey(name);
        }

        internal bool TryAddDecision(string name, Decision decision)
        {
            return TryAddDecision(decisions.Count, name, decision);
        }

        internal bool TryAddDecision(int index, string name, Decision decision)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (decisionDict.TryAdd(name, decision))
            {
                decisions.Insert(index, decision);
                return true;
            }

            return false;
        }

        internal bool TryRemoveDecision(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (decisionDict.Remove(name, out Decision decision))
            {
                decisions.Remove(decision);
                return true;
            }

            return false;
        }

        public Decision GetDecision(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return decisionDict.GetValueOrDefault(name);
        }

        public bool TryGetDecision(string name, out Decision decision)
        {
            decision = null;
            if (string.IsNullOrEmpty(name))
                return false;

            return decisionDict.TryGetValue(name, out decision);
        }

        internal void MoveDecision(int sourceIndex, int destIndex)
        {
            decisions.Move(sourceIndex, destIndex);
        }

        #endregion

        #region Score

        internal DecisionContext MakeDecision(HashSet<UtilityEntity> entities, float minToBeat, ref DecisionMakerContext decisionMakerContext)
        {
            // #if CARLOSLAB_ENABLE_PROFILER
            //             using var _ = Profiler.Sample("DecisionMaker - MakeDecision");
            // #endif

            var bestDecisionContext = DecisionContext.Null;

            if (decisions.Count > 0)
            {
                float bestDecisionScore = minToBeat;

                foreach (Decision decision in decisions)
                {
                    var targets = decision.HasNoTarget ? null : decision.TargetFilters.Count == 0 ? entities : decision.GetFilteredTargets();

                    var decisionContext = CalculateDecisionScore(decision, targets, bestDecisionScore, ref decisionMakerContext);

                    float decisionScore = decisionContext.FinalScore;

                    if (decisionScore > bestDecisionScore)
                    {
                        bestDecisionScore = decisionScore;
                        bestDecisionContext = decisionContext;
                    }

                    if (bestDecisionContext == DecisionContext.Null)
                    {
                        bestDecisionContext = decisionContext;
                    }
                }
            }

            // UtilityIntelligenceConsole.Instance.Log($"Agent: {Agent.Name} DecisionMaker Change Decision From: {CurrentDecision?.Name} To: {bestDecision?.Name}");

            decisionMakerContext.Score = bestDecisionContext.FinalScore;
            decisionMakerContext.BestDecision = bestDecisionContext.Decision;

            return bestDecisionContext;
        }

        private DecisionContext CalculateDecisionScore(Decision decision, HashSet<UtilityEntity> targets, float minToBeat, ref DecisionMakerContext decisionMakerContext)
        {
            // using var _ = Profiler.Sample("CalculateScore - Decision");

            DecisionContext bestDecisionContext;

            if (targets != null)
                bestDecisionContext = CalculateDecisionScoreWithTargets(decision, targets, minToBeat);
            else
                bestDecisionContext = CalculateDecisionScoreWithoutTargets(decision, minToBeat);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            decisionMakerContext.AddDecisionContext(decision.Name, in bestDecisionContext);
#endif
            return bestDecisionContext;
        }

        private DecisionContext CalculateDecisionScoreWithTargets(Decision decision, HashSet<UtilityEntity> targets, float minToBeat)
        {
            var bestDecisionContext = DecisionContext.Null;

            // float finalDecisionScore = 0.0f;

            if (targets.Count > 0)
            {
                float bestDecisionScore = minToBeat;

                foreach (UtilityEntity target in targets)
                {
                    DecisionContext currentContext = new(decision, target, this);
                    float decisionScore = decision.CalculateScore(bestDecisionScore, ref currentContext);

                    if (decisionScore > bestDecisionScore)
                    {
                        bestDecisionScore = decisionScore;
                        bestDecisionContext = currentContext;
                    }

                    if (bestDecisionContext == DecisionContext.Null)
                    {
                        bestDecisionContext = currentContext;
                    }
                }
            }
            else
            {
                bestDecisionContext = new(decision, null, this);
            }

            return bestDecisionContext;
        }

        private DecisionContext CalculateDecisionScoreWithoutTargets(Decision decision, float minToBeat)
        {
            DecisionContext currentContext = new(decision, null, this);

            decision.CalculateScore(minToBeat, ref currentContext);

            return currentContext;
        }

        #endregion

        #region Context

        public event Action<DecisionMakerContext> ContextChanged;

        private DecisionMakerContext context;

        public DecisionMakerContext Context
        {
            get => context;
            private set
            {
                context = value;
                ContextChanged?.Invoke(context);
            }
        }

        private DecisionMakerContext nextContext;

        public DecisionMakerContext NextContext
        {
            get => nextContext;
            internal set
            {
                nextContext = value;

                if (!IsRuntime)
                    Context = nextContext;
            }
        }

        private void ClearContexts()
        {
            context = DecisionMakerContext.Null;
            nextContext = DecisionMakerContext.Null;
        }

        #endregion

        #region State

        protected override void OnEnter()
        {
            Context = nextContext;
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker OnEnter Frame: {WorldTime.Instance.Frame}");
        }

        protected override void ResetState()
        {
            ClearContexts();
        }

        /*

        protected override void OnStart()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker OnStart Frame: {WorldTime.Instance.Frame}");
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker OnAbort Frame: {WorldTime.Instance.Frame}");
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker OnEnd Frame: {WorldTime.Instance.Frame}");
        }
        
        protected override void OnExit()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker OnExit Frame: {WorldTime.Instance.Frame}");
        }
        
		protected override UpdateStatus OnUpdate(float deltaTime)
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker: {Name} OnUpdate CurrentDecision: {State?.Name} Frame: {WorldTime.Instance.Frame}");
            return base.OnUpdate(deltaTime);
        }
        
        protected override void OnStateChanged(Decision oldState, Decision newState)
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker: {Name} OnCurrentStateChanged From State: {oldState?.Name} to State: {newState?.Name} Frame: {WorldTime.Instance.Frame}");
        }
        
        protected override void OnStatusChanged(Status oldStatus, Status newStatus)
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} DecisionMaker: {Name} OnStatusChanged OldStatus: {oldStatus} NewStatus: {newStatus}");
        }
*/

        #endregion

        #region IContainerItem

        private string name;

        public override string Name => name;

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
        }

        public bool IsInContainer => container != null;

        private DecisionMakerContainer container;
        public DecisionMakerContainer Container => container;

        void IContainerItem.HandleItemAdded(IItemContainer container, string name)
        {
            this.container = container as DecisionMakerContainer;
            this.name = name;
        }

        void IContainerItem.HandleItemRemoved()
        {
            this.container = null;
        }

        #endregion
    }
}