// <copyright file="ModelFactory.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common.UI
{
    public static class ModelFactory<TModel> where TModel : class
    {
        public static TModel Create(Type type)
        {
            if (type == null)
                type = typeof(TModel);

            object instance = Activator.CreateInstance(type);
            return instance as TModel;
        }
    }
}