// <copyright file="Consideration.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Consideration : UtilityIntelligenceMemberItem<ConsiderationContainer>, INoTargetItem
    {
        public Consideration(InputNormalization inputNormalization, InfluenceCurve responseCurve)
        {
            this.inputNormalization = inputNormalization;
            this.responseCurve = responseCurve;
        }

        #region Input

        public object RawInput => inputNormalization?.RawInput;
        public Type InputValueType => inputNormalization?.InputValueType;


        private InputNormalization inputNormalization;

        public float NormalizedInput => inputNormalization?.NormalizedInput ?? 0f;
        public InputNormalization InputNormalization
        {
            get => inputNormalization;
            internal set => inputNormalization = value;
        }

        #endregion

        #region Contexts

        private ConsiderationResult noTargetResult;
        private Dictionary<int, ConsiderationResult> targetResults = new(30);

        internal void Reset()
        {
            // score = 0.0f;
            noTargetResult = ConsiderationResult.Null;
            targetResults.Clear();
        }

        private bool TryGetResult(int targetId, out ConsiderationResult result)
        {
            if (targetId < 0)
            {
                result = ConsiderationResult.Null;
                return false;
            }
            return targetResults.TryGetValue(targetId, out result);
        }

        private bool TryAddResult(int targetId, in ConsiderationResult result)
        {
            if (targetId < 0) return false;

            if (targetResults.TryAdd(targetId, result))
                return true;

            return false;
        }

        #endregion

        #region Consideration

        private string category;

        public string Category
        {
            get => category;
            internal set => category = value;
        }

        private bool hasNoTarget;

        public bool HasNoTarget
        {
            get => hasNoTarget;
            internal set
            {
                if (hasNoTarget == value) return;

                hasNoTarget = value;

                var noTargetItems = Container?.NoTargetItems;
                if (noTargetItems != null)
                {
                    if (hasNoTarget)
                        noTargetItems.Add(this);
                    else
                        noTargetItems.Remove(this);
                }
            }
        }

        private InfluenceCurve responseCurve;
        public InfluenceCurve ResponseCurve
        {
            get => responseCurve;
            internal set => responseCurve = value;
        }

        private float score;

        public float Score
        {
            get => score;
            internal set
            {
                score = value;
                ScoreChanged?.Invoke();
            }
        }
        public event Action ScoreChanged;

        #endregion

        #region Calculate Score

        private bool enableCachePerTarget;

        public bool EnableCachePerTarget
        {
            get => enableCachePerTarget;
            internal set => enableCachePerTarget = value;
        }

        internal float CalculateScore(ref ConsiderationContext context)
        {
            ConsiderationResult result;
            if (hasNoTarget)
            {
                CalculateScoreNoTarget(in context, out result);
            }
            else
            {
                if (enableCachePerTarget)
                    CalculateScoreWithCache(in context, out result);
                else
                    CalculateScoreWithoutCache(in context, out result);

            }

            context.SetResult(in result);

            Score = result.Score;
            return result.Score;
        }

        private void CalculateScoreNoTarget(in ConsiderationContext context, out ConsiderationResult result)
        {
            if (noTargetResult == ConsiderationResult.Null)
                CalculateScoreWithoutCache(in context, out noTargetResult);

            result = noTargetResult;
        }

        private void CalculateScoreWithCache(in ConsiderationContext context, out ConsiderationResult result)
        {
            var targetId = context.Target?.Id ?? -1;

            bool getResultSuccess = TryGetResult(targetId, out result);

            if (!getResultSuccess)
            {
                CalculateScoreWithoutCache(in context, out result);
                TryAddResult(targetId, in result);
            }
            else
            {
                UtilityIntelligenceConsole.Instance.Log($"Ignore Calculation Consideration Name: {Name}");
            }
        }

        private void CalculateScoreWithoutCache(in ConsiderationContext context, out ConsiderationResult result)
        {
            result = new(this);

            if (inputNormalization != null)
            {
                InputNormalizationContext inputNormalizationContext = new(inputNormalization, in context);

                InputNormalizationResult inputNormalizationResult = inputNormalization.CalculateNormalizedInput(in inputNormalizationContext);

                result.NormalizedInput = inputNormalizationResult.NormalizedInput;
                result.RawInput = inputNormalizationResult.RawInput;
            }

            result.Score = InfluenceCurveUtils.Evaluate(result.NormalizedInput, in responseCurve);
        }

        #endregion

    }
}