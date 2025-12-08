// <copyright file="Decision.Targets.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Extensions;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision
    {
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

        #region Targets

        private readonly Dictionary<string, TargetFilter> targetFilterDict = new();
        private readonly List<TargetFilter> targetFilters = new();
        public IReadOnlyList<TargetFilter> TargetFilters => targetFilters;
        private HashSet<UtilityEntity> filteredTargets = new(20);
        private HashSet<UtilityEntity> intersectedTargets = new(10);

        internal HashSet<UtilityEntity> GetFilteredTargets()
        {
            //#if CARLOSLAB_ENABLE_PROFILER
            //            using var _ = Profiler.Sample("Decision - GetFilteredTargets");
            //#endif
            if (hasNoTarget || !Intelligence.IsRuntime) return null;

            filteredTargets.Clear();

            for (int targetFilterIndex = 0; targetFilterIndex < targetFilters.Count; targetFilterIndex++)
            {
                var targetCache = targetFilters[targetFilterIndex].TargetCache;

                if (targetFilterIndex == 0)
                {
                    for (int targetIndex = 0; targetIndex < targetCache.Count; targetIndex++)
                    {
                        UtilityEntity target = targetCache[targetIndex];
                        filteredTargets.Add(target);
                    }
                }
                else
                {
                    IntersectTargets(filteredTargets, targetCache, intersectedTargets);
                    filteredTargets.Clear();
                    foreach (var target in intersectedTargets)
                    {
                        filteredTargets.Add(target);
                    }

                    intersectedTargets.Clear();
                }
            }

            return filteredTargets;
        }

        private void IntersectTargets(HashSet<UtilityEntity> targets1, IReadOnlyList<UtilityEntity> targets2,
            HashSet<UtilityEntity> intersectedTargets)
        {
#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("IntersectTargets - Decision");
#endif
            for (int index = 0; index < targets2.Count; index++)
            {
                UtilityEntity target = targets2[index];
                if (targets1.Contains(target))
                {
                    intersectedTargets.Add(target);
                }
            }
        }

        public bool HasTargetFilter(string name)
        {
            return targetFilterDict.ContainsKey(name);
        }

        internal bool TryAddTargetFilter(string name, TargetFilter targetFilter)
        {
            return TryAddTargetFilter(targetFilters.Count, name, targetFilter);
        }

        internal bool TryAddTargetFilter(int index, string name, TargetFilter targetFilter)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (targetFilterDict.TryAdd(name, targetFilter))
            {
                targetFilters.Insert(index, targetFilter);
                return true;
            }

            return false;
        }

        internal bool TryRemoveTargetFilter(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (targetFilterDict.Remove(name, out TargetFilter targetFilter))
            {
                targetFilters.Remove(targetFilter);
                return true;
            }

            return false;
        }

        public bool TryGetTargetFilter(string name, out TargetFilter targetFilter)
        {
            targetFilter = null;
            if (string.IsNullOrEmpty(name))
                return false;

            if (targetFilterDict.TryGetValue(name, out targetFilter))
                return true;

            return false;
        }

        internal void MoveTargetFilter(int sourceIndex, int destIndex)
        {
            targetFilters.Move(sourceIndex, destIndex);
        }

        #endregion
    }
}