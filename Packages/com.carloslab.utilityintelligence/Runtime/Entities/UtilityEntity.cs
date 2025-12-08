// <copyright file="UtilityEntity.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityEntity : Entity<UtilityEntity, UtilityWorld>
    {
        public static readonly UtilityEntity Null = new();

        public IImapEntity ImapEntity { get; internal set; }
    }
}