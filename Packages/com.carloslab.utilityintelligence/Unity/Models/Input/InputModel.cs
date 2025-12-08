// <copyright file="InputModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;
using System;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<InputModel>))]
    public class InputModel : GenericModelItem<InputContainerModel, Input>, IContainerItemValue
    {
        public string Category
        {
            get => (string)GetValue(nameof(Category));
            internal set => SetValue(nameof(Category), value);
        }
        public bool HasNoTarget
        {
            get => (bool)GetValue(nameof(HasNoTarget));
            internal set => SetValue(nameof(HasNoTarget), value);
        }

        public bool EnableCachePerTarget
        {
            get => (bool)GetValue(nameof(EnableCachePerTarget));
            internal set => SetValue(nameof(EnableCachePerTarget), value);
        }

        public Type ValueType => Runtime.ValueType;
        public object ValueObject => Runtime.ValueObject;

    }
}