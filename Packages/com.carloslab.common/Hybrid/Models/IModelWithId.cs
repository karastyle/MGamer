// <copyright file="IModelWithId.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IModelWithId : IModel
    {
        string Id { get; set; }
    }
}