// <copyright file="IScoreViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence.UI
{
    public interface IScoreViewModel : IViewModel
    {
        float Score { get; }
    }
}