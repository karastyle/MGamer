// <copyright file="INoTargetItem.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public interface INoTargetItem : IContainerItem
    {
        public bool HasNoTarget { get; }
    }
}