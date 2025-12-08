// <copyright file="GenericModelFactory.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;

namespace CarlosLab.UtilityIntelligence
{
    public static class GenericModelFactory
    {
        public static TModelWithId CreateWithId<TModelWithId>(Type runtimeType, Blackboard blackboard) where TModelWithId : GenericModel, IGenericModel, IModelWithId, new()
        {
            TModelWithId model = Create<TModelWithId>(runtimeType, blackboard);
            model.Id = Guid.NewGuid().ToString();
            return model;
        }

        public static TModel Create<TModel>(Type runtimeType, Blackboard blackboard) where TModel : GenericModel, IGenericModel, new()
        {
            TModel model = new();
            model.Init(runtimeType);

            if (blackboard == null) return model;

            model.SetVariableReference(blackboard);

            return model;
        }
    }
}