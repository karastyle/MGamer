// <copyright file="CustomObjectSerializer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace CarlosLab.Common
{
    public abstract class CustomObjectSerializer
    {
        private JsonSerializerSettings settings;

        private JsonSerializer serializer;

        private JsonSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    var settings = CreateSettings();
                    serializer = JsonSerializer.Create(settings);
                }

                return serializer;
            }
        }

        protected abstract JsonSerializerSettings CreateSettings();

        public string Serialize(object obj, Formatting formatting = Formatting.None)
        {
            if (obj == null) return "null";

            StringBuilder sb = new(256);
            StringWriter sw = new(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new(sw))
            {
                jsonWriter.Formatting = formatting;

                Serializer.Serialize(jsonWriter, obj);
            }

            return sw.ToString();
        }

        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                using JsonTextReader reader = new(new StringReader(json));
                return Serializer.Deserialize<T>(reader);
            }
            catch (Exception e)
            {
                StaticConsole.LogException(e);
                return default;
            }
        }
    }

    public abstract class CustomObjectSerializer<TAsset> : CustomObjectSerializer
        where TAsset : IDataAsset
    {
        private readonly TAsset asset;

        public CustomObjectSerializer(TAsset asset)
        {
            this.asset = asset;
        }

        public TAsset Asset => asset;
    }
}