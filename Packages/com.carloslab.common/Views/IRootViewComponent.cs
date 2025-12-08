// <copyright file="IRootViewComponent.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IRootViewComponent
    {
        bool IsRuntime { get; }
        bool IsRuntimeUI { get; }
    }
}