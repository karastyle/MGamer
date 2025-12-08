// <copyright file="IView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public interface IView
    {
        event Action Shown;
        event Action Hidden;
        void Show(bool show);
    }

    public interface IView<TViewModel> : IView
        where TViewModel : class, IViewModel
    {
        TViewModel ViewModel { get; }
        event Action<TViewModel> ViewModelChanged;
        void UpdateView(TViewModel viewModel);
    }
}