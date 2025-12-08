// <copyright file="IRootViewModelMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IRootViewModelMember : IRootViewModelComponent
    {

    }
    public interface IRootViewModelMember<TRootViewModel> : IRootViewModelMember
        where TRootViewModel : class, IRootViewModel
    {
        TRootViewModel RootViewModel { get; set; }
    }
}