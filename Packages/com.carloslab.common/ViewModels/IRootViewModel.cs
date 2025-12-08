// <copyright file="IRootViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IRootViewModel : IRootViewModelComponent
    {
        bool IsRuntimeEditorOpening { get; }
        int EditorOpeningCount { get; }
    }

    public interface IRootViewModel<TAsset> : IRootViewModel
        where TAsset : class, IDataAsset
    {
        void Init(TAsset asset);
    }
}