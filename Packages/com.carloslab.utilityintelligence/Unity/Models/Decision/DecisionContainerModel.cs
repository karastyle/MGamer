// <copyright file="DecisionContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<DecisionContainerModel, DecisionModel>))]
    public class DecisionContainerModel : ItemContainerModel<DecisionModel, Decision, DecisionContainer>
    {
    }
}