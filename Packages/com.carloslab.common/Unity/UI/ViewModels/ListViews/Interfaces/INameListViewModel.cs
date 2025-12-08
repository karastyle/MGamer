// <copyright file="INameListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public interface INameListViewModel : IListViewModel
    {
        bool Contains(string name);
    }
}