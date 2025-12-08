// ShaderVariantCollectionHelper.cs
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace EasyShaderCollector
{
    public static class ShaderVariantCollectionHelper
    {
        public static void ClearCurrentShaderVariantCollection()
        {
            EditorHelper.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
        }

        public static void SaveCurrentShaderVariantCollection(string savePath)
        {
            EditorHelper.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
        }

        public static int GetCurrentShaderVariantCollectionShaderCount()
        {
            return (int)EditorHelper.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
        }

        public static int GetCurrentShaderVariantCollectionVariantCount()
        {
            return (int)EditorHelper.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
        }

        public static string GetShaderVariantCount(string assetPath)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
            var variantCount = EditorHelper.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
            return variantCount.ToString();
        }
    }
}