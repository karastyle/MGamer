// <copyright file="TargetFilter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class TargetFilter : UtilityIntelligenceMemberItem<TargetFilterContainer>
    {
        #region Target Filter

        [DataMember(Name = nameof(Category))]
        private string category;

        public string Category
        {
            get => category;
            internal set => category = value;
        }

        private readonly List<UtilityEntity> targetCache = new(20);

        public IReadOnlyList<UtilityEntity> TargetCache => targetCache;

        public bool FilterTarget(UtilityEntity target)
        {
            bool result = OnFilterTarget(target);
            if (result)
                targetCache.Add(target);
            return result;
        }

        protected abstract bool OnFilterTarget(UtilityEntity target);

        public void Reset()
        {
            targetCache.Clear();
        }

        #endregion
    }
}