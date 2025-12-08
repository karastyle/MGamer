// <copyright file="InputNormalizationContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<InputNormalizationContainerModel, InputNormalizationModel>))]
    public class InputNormalizationContainerModel : ItemContainerModel<InputNormalizationModel, InputNormalization, InputNormalizationContainer>
    {

    }
}