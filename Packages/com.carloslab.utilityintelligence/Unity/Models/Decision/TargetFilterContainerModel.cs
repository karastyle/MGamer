// <copyright file="TargetFilterContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<TargetFilterContainerModel, TargetFilterModel>))]
    public class TargetFilterContainerModel : ItemContainerModel<TargetFilterModel, TargetFilter, TargetFilterContainer>
    {
    }
}