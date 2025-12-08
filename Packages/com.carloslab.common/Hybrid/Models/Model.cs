// <copyright file="Model.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    public abstract class Model : IModel
    {
        public IDataAsset Asset { get; private set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is IDataAsset asset)
            {
                Asset = asset;
            }
        }
    }
    public abstract class Model<TRuntime> : Model, IModel<TRuntime>
        where TRuntime : class, IRuntimeObject
    {
        public abstract TRuntime Runtime { get; }
    }
}