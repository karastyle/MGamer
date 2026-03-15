// ShaderVariantCollectorConfig.cs
using UnityEngine;

namespace EasyShaderCollector
{
    [CreateAssetMenu(fileName = "ShaderVariantCollectorConfig", menuName = "AssetBundle/ShaderVariantCollectorConfig")]
    public class ShaderVariantCollectorConfig : ScriptableObject
    {
        public AssetBundleConfig assetBundleConfig;
        public string saveFolder = "Assets";
        public string saveFileName = "MyShaderVariants";
        public int processCapacity = 1000;

        public string GetSavePath()
        {
            string fileName = saveFileName;
            if (!fileName.EndsWith(".shadervariants"))
                fileName += ".shadervariants";
            return saveFolder.TrimEnd('/') + "/" + fileName;
        }
    }
}