// <copyright file="ConsiderationContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<ConsiderationContainerModel, ConsiderationModel>))]
    public class ConsiderationContainerModel : ItemContainerModel<ConsiderationModel, Consideration, ConsiderationContainer>
    {
    }
}