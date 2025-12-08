// <copyright file="DataSerializer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace CarlosLab.Common
{
    public static class DataSerializer
    {
        private static readonly JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            SerializationBinder = new DataSerializationBinder(),
            ContractResolver = new JsonContractResolver()
        };

        public static bool TrySerialize(object model, ref StreamingContext context, out string serializedModel)
        {
            serializedModel = null;
            if (model == null)
                return false;

            try
            {
                settings.Context = context;
                serializedModel = JsonConvert.SerializeObject(model, settings);
                return true;
            }
            catch (JsonException e)
            {
                StaticConsole.LogException(e);
            }

            return false;
        }

        public static string Format(string serializedModel)
        {
            return JToken.Parse(serializedModel).ToString(Formatting.Indented);
        }

        public static bool TryDeserialize<T>(string json, ref StreamingContext context, out T model)
        {
            model = default;
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                settings.Context = context;
                model = JsonConvert.DeserializeObject<T>(json, settings);
                return true;
            }
            catch (JsonException e)
            {
                StaticConsole.LogException(e);
            }

            return false;
        }
    }
}