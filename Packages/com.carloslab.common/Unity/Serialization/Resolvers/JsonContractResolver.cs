// <copyright file="JsonContractResolver.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace CarlosLab.Common
{
    public class JsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo) property.ShouldSerialize = _ => false;

            return property;
        }
    }
}