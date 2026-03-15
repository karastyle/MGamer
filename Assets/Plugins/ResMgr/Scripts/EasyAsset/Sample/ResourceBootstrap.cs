// ResourceBootstrap.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EasyTools
{
    public class ResourceBootstrap : MonoBehaviour
    {
        [Header("运行模式")] 
        public EPlayMode playMode = EPlayMode.EditorSimulateMode;

        [Header("Shader变体配置")]
        public string shaderVariantLocation = "MyShaderVariants";
        
        /// <summary>
        /// 初始化接口，由外部调用
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log($"[ResourceBootstrap] 开始初始化EasyAsset: {playMode}");

            yield return EasyAsset.Instance.InitializeAsync(playMode);

            Debug.Log($"[ResourceBootstrap] EasyAsset初始化完成");

            yield return InitResource();
        }

        public IEnumerable InitResource()
        {
            // 加载shader变体集合以预热
            yield return WarmupShaders();

            Debug.Log("[ResourceBootstrap] Shader预热完成");
        }
        
        private IEnumerator WarmupShaders()
        {
            Debug.Log("[ResourceBootstrap] 开始加载Shader变体集合...");

            AssetHandle handle = EasyAsset.Instance.LoadAssetAsync(shaderVariantLocation);
            yield return handle.WaitForCompletion();

            if (handle.Status == EProviderStatus.Succeed)
            {
                ShaderVariantCollection collection = handle.AssetObject<ShaderVariantCollection>();
            
                if (collection != null)
                {
                    Debug.Log($"[ResourceBootstrap] 加载成功: {collection.name}, 开始预热...");
                
                    collection.WarmUp();
                
                    Debug.Log($"[ResourceBootstrap] Shader变体预热完成, 变体数量: {collection.shaderCount}");
                }
                else
                {
                    Debug.LogError("[ResourceBootstrap] ShaderVariantCollection加载失败");
                }

                handle.Release();
            }
            else
            {
                Debug.LogError($"[ResourceBootstrap] 加载Shader变体失败: {handle.LastError}");
                handle.Release();
            }
        }
    }
}