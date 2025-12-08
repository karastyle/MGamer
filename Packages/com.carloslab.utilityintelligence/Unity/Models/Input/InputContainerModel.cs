// <copyright file="InputContainerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<InputContainerModel, InputModel>))]
    public class InputContainerModel : ItemContainerModel<InputModel, Input, InputContainer>
    {
    }
}