// <copyright file="IModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IModel
    {
        public IDataAsset Asset { get; }
    }

    public interface IModel<TRuntime> : IModel
        where TRuntime : class, IRuntimeObject
    {
        TRuntime Runtime { get; }
    }
}