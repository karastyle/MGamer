// <copyright file="Input.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class Input : UtilityIntelligenceMemberItem<InputContainer>, IContainerItemValue, INoTargetItem
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

        #region Value

        public event Action ValueChanged;
        public abstract object ValueObject { get; internal set; }

        public abstract Type ValueType { get; }

        protected void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
        }

        #endregion

        public abstract void Reset();
    }

    public abstract class Input<TValue> : Input, IContainerItemValue<TValue>
    {
        #region Value

        private TValue value;

        public TValue Value
        {
            get => value;
            private set
            {
                this.value = value;
                RaiseValueChanged();
            }
        }

        public override object ValueObject
        {
            get => value;
            internal set
            {
                if (value is TValue tValue)
                    this.value = tValue;
            }
        }

        public override Type ValueType => typeof(TValue);

        #endregion

        #region Contexts

        private InputResult<TValue> noTargetResult;
        private Dictionary<int, InputResult<TValue>> targetResults = new(40);

        public override void Reset()
        {
            noTargetResult = InputResult<TValue>.Null;
            targetResults.Clear();
        }

        private bool TryGetResult(int targetId, out InputResult<TValue> result)
        {
            if (targetId < 0)
            {
                result = InputResult<TValue>.Null;
                return false;
            }

            return targetResults.TryGetValue(targetId, out result);
        }

        private bool TryAddResult(int targetId, InputResult<TValue> result)
        {
            if (targetId < 0) return false;

            if (targetResults.TryAdd(targetId, result))
                return true;

            return false;
        }

        #endregion

        #region GetRawInput

        internal TValue GetRawInput(in InputContext context)
        {
            InputResult<TValue> result;
            if (hasNoTarget)
            {
                GetRawInputNoTarget(in context, out result);
            }
            else
            {
                if (enableCachePerTarget)
                    GetRawInputWithCache(in context, out result);
                else
                    GetRawInputWithoutCache(in context, out result);
            }

            return result.RawInput;
        }

        private void GetRawInputNoTarget(in InputContext context, out InputResult<TValue> result)
        {
            if (noTargetResult == InputResult<TValue>.Null)
                GetRawInputWithoutCache(in context, out noTargetResult);

            result = noTargetResult;
        }

        private void GetRawInputWithCache(in InputContext context, out InputResult<TValue> result)
        {
            var targetId = context.Target?.Id ?? -1;

            bool getResultSuccess = TryGetResult(targetId, out result);

            if (!getResultSuccess)
            {
                GetRawInputWithoutCache(in context, out result);
                TryAddResult(targetId, result);
            }
            else
            {
                UtilityIntelligenceConsole.Instance.Log($"Ignore Calculation Input Name: {Name}");
            }
        }

        private void GetRawInputWithoutCache(in InputContext context, out InputResult<TValue> result)
        {
            result = new(this);

            TValue value;

            if (IsRuntime)
            {
                value = OnGetRawInput(in context);
                Value = value;
            }
            else
            {
                value = this.value;
            }

            result.RawInput = value;
        }

        protected virtual TValue OnGetRawInput(in InputContext context)
        {
            return value;
        }

        #endregion
    }
}