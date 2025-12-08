// <copyright file="RootView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public class RootView<TRootViewModel> : BaseView<TRootViewModel>, IRootView
        where TRootViewModel : class, IRootViewModel
    {
        public RootView(bool isRuntimeUI, string visualAssetPath) : base(visualAssetPath)
        {
            IsRuntimeUI = isRuntimeUI;
        }

        public bool IsRuntime => ViewModel?.IsRuntime ?? false;
        public bool IsRuntimeUI { get; set; }
    }
}