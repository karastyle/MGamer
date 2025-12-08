// <copyright file="InputNormalization.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class InputNormalization : UtilityIntelligenceMemberItem<InputNormalizationContainer>, INoTargetItem
    {
        [DataMember(Name = nameof(Category))]
        private string category;

        public string Category
        {
            get => category;
            internal set => category = value;
        }

        #region HasNoTarget

        [DataMember(Name = nameof(HasNoTarget))]
        protected bool hasNoTarget;
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

        [DataMember(Name = nameof(EnableCachePerTarget))]
        protected bool enableCachePerTarget;
        public bool EnableCachePerTarget
        {
            get => enableCachePerTarget;
            internal set => enableCachePerTarget = value;
        }

        #endregion

        #region NormalizedInput

        private float normalizedInput;
        public float NormalizedInput
        {
            get => normalizedInput;

            protected set
            {
                normalizedInput = value;
                NormalizedInputChanged?.Invoke();
            }
        }

        public event Action NormalizedInputChanged;


        internal abstract InputNormalizationResult CalculateNormalizedInput(in InputNormalizationContext context);

        #endregion

        #region Input

        [DataMember(Name = nameof(Input))]
        private string inputName;

        protected Input input;

        public Input Input
        {
            get => input;
            internal set => input = value;
        }

        public InputContainer InputContainer => Intelligence?.InputContainer;

        public object RawInput => Input?.ValueObject;

        public Type InputType => Input?.GetType();
        public abstract Type InputValueType { get; }


        #endregion

        #region Contexts

        protected InputNormalizationResult noTargetResult;
        private Dictionary<int, InputNormalizationResult> targetResults = new(30);

        public void Reset()
        {
            // normalizedInput = 0.0f;
            noTargetResult = InputNormalizationResult.Null;
            targetResults.Clear();
        }

        internal bool TryGetResult(int targetId, out InputNormalizationResult result)
        {
            if (targetId < 0)
            {
                result = InputNormalizationResult.Null;
                return false;
            }
            return targetResults.TryGetValue(targetId, out result);
        }

        internal bool TryAddResult(int targetId, in InputNormalizationResult result)
        {
            if (targetId < 0) return false;

            if (targetResults.TryAdd(targetId, result))
                return true;

            return false;
        }

        #endregion
    }

    public abstract class InputNormalization<TValue> : InputNormalization
    {
        public override Type InputValueType => typeof(TValue);

        internal override InputNormalizationResult CalculateNormalizedInput(in InputNormalizationContext context)
        {
            InputNormalizationResult result;
            if (hasNoTarget)
            {
                CalculateNormalizedInputNoTarget(in context, out result);
            }
            else
            {
                if (enableCachePerTarget)
                    CalculateNormalizedInputWithCache(in context, out result);
                else
                    CalculateNormalizedInputWithoutCache(in context, out result);
            }

            NormalizedInput = result.NormalizedInput;

            return result;
        }

        private void CalculateNormalizedInputNoTarget(in InputNormalizationContext context, out InputNormalizationResult result)
        {
            if (noTargetResult == InputNormalizationResult.Null)
                CalculateNormalizedInputWithoutCache(in context, out noTargetResult);

            result = noTargetResult;
        }

        private void CalculateNormalizedInputWithCache(in InputNormalizationContext context, out InputNormalizationResult result)
        {
            var targetId = context.Target?.Id ?? -1;

            bool getResultSuccess = TryGetResult(targetId, out result);

            if (!getResultSuccess)
            {
                CalculateNormalizedInputWithoutCache(in context, out result);
                TryAddResult(targetId, in result);
            }
            else
            {
                UtilityIntelligenceConsole.Instance.Log($"Ignore Calculation InputNormalization Name: {Name}");
            }
        }

        private void CalculateNormalizedInputWithoutCache(in InputNormalizationContext context, out InputNormalizationResult result)
        {
            result = new(this);

            TValue rawInput = default;

            if (input is Input<TValue> inputWithValue)
            {
                InputContext inputContext = new(input, in context);
                rawInput = inputWithValue.GetRawInput(in inputContext);
            }

            result.NormalizedInput = CalculateNormalizedInput(rawInput, in context);

            if (IsEditorOpening)
                result.RawInput = rawInput;
        }

        private float CalculateNormalizedInput(TValue rawInput, in InputNormalizationContext context)
        {
            float normalizedInput = OnCalculateNormalizedInput(rawInput, in context);
            normalizedInput = Math.Clamp(normalizedInput, 0.0f, 1.0f);
            return normalizedInput;
        }

        protected abstract float OnCalculateNormalizedInput(TValue rawInput, in InputNormalizationContext context);
    }
}