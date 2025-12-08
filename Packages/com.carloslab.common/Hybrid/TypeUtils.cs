// <copyright file="TypeUtils.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CarlosLab.Common
{
    public static class TypeUtils
    {
        private static Assembly[] assemblyCache;
        private static readonly Dictionary<string, Type> TypeCache = new();

        private static Assembly[] AssemblyCache
        {
            get
            {
                if (assemblyCache == null)
                    assemblyCache = AppDomain.CurrentDomain.GetAssemblies();

                return assemblyCache;
            }
        }

        public static Type GetType(string typeName)
        {
            if (TypeCache.TryGetValue(typeName, out Type type))
                return type;

            foreach (Assembly assembly in AssemblyCache)
            {
                type = GetType(assembly, typeName);
                if (type != null) break;
            }
            return type;
        }

        private static Type GetType(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName);
            if (type == null)
                type = GetGenericType(assembly, typeName);

            if (type != null)
                TypeCache.Add(typeName, type);

            return type;
        }

        private static Type GetGenericType(Assembly assembly, string typeName)
        {
            Type type = null;
            int index = typeName.IndexOf("`", StringComparison.Ordinal);
            if (index >= 0)
            {
                index += 2;
                string genericTypeName = typeName.Substring(0, index);

                string argumentTypeString = typeName.Substring(index + 1, typeName.Length - index - 2);

                //-----------Remove [] in old versions------------------
                if (argumentTypeString[0] == '[')
                    argumentTypeString = argumentTypeString.Substring(1, argumentTypeString.Length - 2);
                //----------------------------------------------------------------

                string[] argumentTypeNames = argumentTypeString.Split(",");
                List<Type> argumentTypes = new();
                foreach (string argumentTypeName in argumentTypeNames)
                {
                    Type argumentType = GetType(argumentTypeName);

                    if (argumentType != null)
                        argumentTypes.Add(argumentType);
                }

                Type genericType = assembly.GetType(genericTypeName);

                if (genericType != null && argumentTypes.Count == argumentTypeNames.Length)
                    type = genericType.MakeGenericType(argumentTypes.ToArray());
            }

            return type;
        }
    }
}