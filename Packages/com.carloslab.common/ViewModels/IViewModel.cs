// <copyright file="IViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public interface IViewModel
    {
        string Id { get; }
        public object ModelObject { get; }
        void ClearModel();
        event Action<IModel> ModelChanged;
    }

    public interface IViewModel<TModel> : IViewModel
    {
        TModel Model { get; set; }

        void Init(TModel model);
        void Deinit();
    }
}