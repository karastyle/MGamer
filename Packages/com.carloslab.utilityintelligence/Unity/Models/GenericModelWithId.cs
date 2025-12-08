// <copyright file="GenericModelWithId.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class GenericModelWithId<TRuntime> : GenericModel<TRuntime>, IModelWithId
        where TRuntime : class, IRuntimeObject
    {
        public string Id
        {
            get => GetValue(nameof(Id)) as string;
            internal set => SetValue(nameof(Id), value);
        }
        string IModelWithId.Id
        {
            get => GetValue(nameof(Id)) as string;
            set => SetValue(nameof(Id), value);
        }
    }
}