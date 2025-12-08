// <copyright file="IStatusViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public interface IStatusViewModel : IViewModel
    {
        event Action<Status> StatusChanged;
        Status CurrentStatus { get; }
    }
}