// <copyright file="IRootViewMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IRootViewMember : IRootViewComponent
    {

    }
    public interface IRootViewMember<TRootView> : IRootViewMember
        where TRootView : class, IRootView
    {
        TRootView RootView { get; set; }
    }
}