// <copyright file="JsonReaderExtension.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;

namespace CarlosLab.Common.Extensions
{
    public static class JsonReaderExtension
    {
        public static void SkipCurrentObject(this JsonReader reader)
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                reader.Read();
            }
        }

        public static void Skip(this JsonReader reader, JsonToken tokenType, int depth)
        {
            if (IsStartToken(tokenType))
            {
                while (reader.Read() && (depth < reader.Depth))
                {
                }
            }
        }

        private static bool IsStartToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.StartConstructor:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsEndToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                case JsonToken.EndConstructor:
                    return true;
                default:
                    return false;
            }
        }
    }
}