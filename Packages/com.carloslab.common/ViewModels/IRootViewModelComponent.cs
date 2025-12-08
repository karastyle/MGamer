// <copyright file="IRootViewModelComponent.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public interface IRootViewModelComponent : IViewModel
    {
        int DataVersion { get; }
        bool IsEditorOpening { get; }
        bool IsRuntime { get; }
        void Record(string name, Action action, bool save = false);
    }
}