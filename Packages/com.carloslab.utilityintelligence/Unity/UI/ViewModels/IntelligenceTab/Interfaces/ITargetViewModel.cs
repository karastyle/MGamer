// <copyright file="ITargetViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence.UI
{
    public interface ITargetViewModel : IViewModel
    {
        string TargetName { get; }
    }
}