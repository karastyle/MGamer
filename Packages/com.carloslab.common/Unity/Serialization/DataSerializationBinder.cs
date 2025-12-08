// <copyright file="DataSerializationBinder.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json.Serialization;
using System;

namespace CarlosLab.Common
{
    public sealed class DataSerializationBinder : ISerializationBinder
    {
        #region ISerializationBinder

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.ToString();
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            Type type = TypeUtils.GetType(typeName);
            return type;
        }

        #endregion
    }
}