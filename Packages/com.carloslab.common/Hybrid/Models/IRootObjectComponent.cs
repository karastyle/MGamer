// <copyright file="IRootObjectComponent.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IRootObjectComponent : IRuntimeObject
    {
        bool IsEditorOpening { get; }
        bool IsRuntime { get; }
    }
}