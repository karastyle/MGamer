// <copyright file="ActionModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<ActionModel>))]
    public class ActionModel : GenericModelWithId<ActionTask>
    {

    }
}