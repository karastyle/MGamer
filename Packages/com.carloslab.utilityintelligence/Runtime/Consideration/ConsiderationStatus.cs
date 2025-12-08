// <copyright file="ConsiderationStatus.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public enum ConsiderationStatus
    {
        Start = Status.Start,
        Executed = Status.Success,
        Discarded = Status.Aborted
    }
}