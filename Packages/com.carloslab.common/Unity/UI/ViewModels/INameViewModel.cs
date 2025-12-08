// <copyright file="INameViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public interface INameViewModel : IViewModel
    {
        string Name { get; set; }
    }
}