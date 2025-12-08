// <copyright file="IValueViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public interface IValueViewModel : IViewModel
    {
        object ValueObject { get; set; }
        Type ValueType { get; }
    }
}