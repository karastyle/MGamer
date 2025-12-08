// <copyright file="BlackboardModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;

namespace CarlosLab.Common
{
    [JsonConverter(typeof(ItemContainerConverter<BlackboardModel, Variable>))]
    public class BlackboardModel : ItemContainerModel<Variable, Blackboard>
    {
    }
}