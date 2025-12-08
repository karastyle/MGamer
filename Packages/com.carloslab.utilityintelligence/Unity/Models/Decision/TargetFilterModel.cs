// <copyright file="TargetFilterModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<TargetFilterModel>))]
    public class TargetFilterModel : GenericModelItem<TargetFilterContainerModel, TargetFilter>
    {
        public string Category
        {
            get => (string)GetValue(nameof(Category));
            internal set => SetValue(nameof(Category), value);
        }
    }
}