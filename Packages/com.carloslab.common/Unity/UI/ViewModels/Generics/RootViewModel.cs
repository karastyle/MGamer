// <copyright file="RootViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public class RootViewModel<TModel, TAsset> : ViewModel<TModel>, IRootViewModel<TAsset>
        where TModel : class, IRootModel
        where TAsset : class, IDataAsset<TModel>
    {

        public void Init(TAsset asset)
        {
            if (asset == null) return;
            this.asset = asset;

            base.Init(asset.Model);
        }

        private TAsset asset;

        public TAsset Asset => asset;

        public void Record(string name, Action action, bool save = false)
        {
            asset?.Record(name, action, save);
        }

        public int DataVersion => Model?.DataVersion ?? 0;

        public bool IsRuntimeEditorOpening => Asset.IsRuntimeEditorOpening;

        public bool IsEditorOpening => Asset.IsEditorOpening;

        public int EditorOpeningCount => Asset.EditorOpeningCount;

        public bool IsRuntime => Asset.IsRuntime;
    }
}