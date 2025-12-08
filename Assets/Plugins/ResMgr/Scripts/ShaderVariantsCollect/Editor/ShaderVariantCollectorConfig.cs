// ShaderVariantCollectorConfig.cs
using UnityEngine;

namespace EasyShaderCollector
{
    [CreateAssetMenu(fileName = "ShaderVariantCollectorConfig", menuName = "AssetBundle/ShaderVariantCollectorConfig")]
    public class ShaderVariantCollectorConfig : ScriptableObject
    {
        public AssetBundleConfig assetBundleConfig;
        public string savePath = "Assets/MyShaderVariants.shadervariants";
        public int processCapacity = 1000;
    }
}