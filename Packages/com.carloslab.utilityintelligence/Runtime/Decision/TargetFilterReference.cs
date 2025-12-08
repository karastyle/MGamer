// <copyright file="TargetFilterReference.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class TargetFilterReference : ItemReference<TargetFilter, TargetFilterContainer>
    {
        public TargetFilterReference(string name, TargetFilterContainer container = null) : base(name, container)
        {
        }
    }
}