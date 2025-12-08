// <copyright file="IMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public interface IMainView<TSubView> : IView
        where TSubView : BaseView, IView
    {
        // void Init(TSubView subView);
        TSubView SubView { get; }

        void InitSubView(TSubView subView);
    }
}