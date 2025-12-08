// <copyright file="IItemContainer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IItemContainer
    {
        int Count { get; }
        bool Contains(string name);
    }
}