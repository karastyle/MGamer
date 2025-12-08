// <copyright file="Decision.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision : UtilityIntelligenceMemberState, INoTargetItem
    {
        private float weight;

        public float Weight
        {
            get => weight;
            internal set => weight = value;
        }

        private string category;

        public string Category
        {
            get => category;
            internal set => category = value;
        }

        private bool enableCachePerTarget;
        public bool EnableCachePerTarget
        {
            get => enableCachePerTarget;
            internal set => enableCachePerTarget = value;
        }

        protected override void OnRootObjectChanged(UtilityIntelligence intelligence)
        {
            task.RootObject = intelligence;
        }

        public Decision(Repeater task)
        {
            this.task = task ?? new Repeater();
        }



        internal void Init()
        {
            UpdateCompensationFactor();
        }



        internal float CalculateScore(float minToBeat, ref DecisionContext context)
        {
            // #if CARLOSLAB_ENABLE_PROFILER
            //             using var _ = Profiler.Sample("Decision - CalculateScore");
            // #endif
            DecisionResult result;
            if (hasNoTarget)
            {
                CalculateScoreNoTarget(minToBeat, ref context, out result);
            }
            else
            {
                if (enableCachePerTarget)
                    CalculateScoreWithCache(minToBeat, ref context, out result);
                else
                    CalculateScoreWithoutCache(minToBeat, ref context, out result);
            }

            context.SetResult(result);

            float finalScore = AddMomentumBonus(result.Score, in context, in this.context);
            context.FinalScore = finalScore;
            return finalScore;
        }

        private void CalculateScoreNoTarget(float minToBeat, ref DecisionContext context, out DecisionResult result)
        {
            if (noTargetResult == DecisionResult.Null)
                CalculateScoreWithoutCache(minToBeat, ref context, out noTargetResult);

            result = noTargetResult;
        }

        private void CalculateScoreWithCache(float minToBeat, ref DecisionContext context, out DecisionResult result)
        {
            var targetId = context.Target?.Id ?? -1;

            bool getResultSuccess = TryGetResult(targetId, out result);

            if (!getResultSuccess)
            {
                CalculateScoreWithoutCache(minToBeat, ref context, out result);

                if (!result.Discarded)
                    TryAddResult(targetId, result);
            }
            else
            {
                // UtilityIntelligenceConsole.Instance.Log($"Ignore Calculation Decision Name: {Name}");
            }
        }

        private void CalculateScoreWithoutCache(float minToBeat, ref DecisionContext context, out DecisionResult result)
        {
            result = new(this);

            float decisionScore = 0.0f;

            // UtilityIntelligenceConsole.Instance.Log($"Decision: {decision.Name} Bonus: {score}");

            if (considerations.Count > 0)
            {
                decisionScore = weight;

                foreach (Consideration consideration in considerations)
                {
                    ConsiderationContext considerationContext = new(consideration, in context);

                    if (decisionScore <= 0.0f || decisionScore <= minToBeat)
                    {
                        considerationContext.CurrentStatus = ConsiderationStatus.Discarded;

                        decisionScore = 0.0f;

                        result.Discarded = true;
                    }
                    else
                    {
                        float considerationScore = consideration.CalculateScore(ref considerationContext);

                        float finalConsiderationScore = AddCompensationFactor(considerationScore);

                        considerationContext.CurrentStatus = ConsiderationStatus.Executed;
                        considerationContext.FinalScore = finalConsiderationScore;

                        decisionScore *= finalConsiderationScore;
                    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    context.AddConsiderationContext(consideration.Name, in considerationContext);
#endif
                }
            }

            // UtilityAIConsole.Instance.Log($"DSE Decision: {Context.Decision.Name} Score: {finalScore}");

            result.Score = decisionScore;
        }

        #region CompensationFactor

        public bool EnableCompensationFactor => rootObject.EnableCompensationFactor;

        private float compensationFactor;

        private void UpdateCompensationFactor()
        {
            compensationFactor = 1.0f - 1.0f / considerations.Count;
        }

        private float AddCompensationFactor(float considerationScore)
        {
            if (EnableCompensationFactor)
            {
                float makeUpValue = (1.0f - considerationScore) * compensationFactor;
                return considerationScore + considerationScore * makeUpValue;
            }

            return considerationScore;
        }

        #endregion

        #region MomentumBonus

        public float MomentumBonus => rootObject.MomentumBonus;

        internal float AddMomentumBonus(float decisionScore, in DecisionContext currentContext, in DecisionContext lastContext)
        {
            var currentDecision = currentContext.Decision;
            var currentTarget = currentContext.Target;

            var lastDecision = lastContext.Decision;
            var lastTarget = lastContext.Target;

            if (currentDecision == lastDecision && currentTarget == lastTarget)
            {
                decisionScore *= MomentumBonus;
            }

            return decisionScore;
        }

        #endregion
    }
}