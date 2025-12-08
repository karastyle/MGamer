using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class CopyDepthFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        // 建议在 Transparents 之后
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();
    private CopyDepthPass copyDepthPass;

    public override void Create()
    {
        copyDepthPass = new CopyDepthPass(settings);
    }

    // 🔧 【核心修改在这里】
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 1. 必须是 Game 视图 (直接把 SceneView 排除掉)
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        // 2. 必须是场景里的 Main Camera
        // 注意：这会排除掉 UI 相机、小地图相机等其他 Game 类型的相机
        if (renderingData.cameraData.camera != Camera.main)
            return;

        renderer.EnqueuePass(copyDepthPass);
    }

    protected override void Dispose(bool disposing)
    {
        copyDepthPass?.Dispose();
    }

    public class CopyDepthPass : ScriptableRenderPass
    {
        private const string PassName = "Copy Depth for HZB";
        private Material copyMaterial;
        
        private static RTHandle s_PreviousFrameDepthHandle;
        
        public static RenderTexture PreviousFrameDepth => s_PreviousFrameDepthHandle?.rt;

        private class PassData
        {
            public Material material;
            public TextureHandle source;
        }

        public CopyDepthPass(Settings settings)
        {
            this.renderPassEvent = settings.renderPassEvent;
            
            Shader copyShader = Shader.Find("Hidden/CustomCopyDepth");
            if (copyShader != null)
            {
                copyMaterial = new Material(copyShader);
            }
            else
            {
                Debug.LogError("[CopyDepthPass] Shader Hidden/CustomCopyDepth not found!");
            }
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (copyMaterial == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // 🔧 【双重保险】这里也可以加一个判断，防止极端情况
            if (cameraData.camera != Camera.main) return;

            TextureHandle cameraDepth = resourceData.cameraDepth;
            if (!cameraDepth.IsValid()) return;

            int width = cameraData.camera.pixelWidth;
            int height = cameraData.camera.pixelHeight;

            if (s_PreviousFrameDepthHandle == null || 
                s_PreviousFrameDepthHandle.rt.width != width || 
                s_PreviousFrameDepthHandle.rt.height != height)
            {
                s_PreviousFrameDepthHandle?.Release();

                s_PreviousFrameDepthHandle = RTHandles.Alloc(
                    width, height, 
                    colorFormat: GraphicsFormat.R32_SFloat, 
                    name: "_PreviousFrameDepth_HZB"
                );
            }

            TextureHandle destinationHandle = renderGraph.ImportTexture(s_PreviousFrameDepthHandle);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
            {
                passData.material = copyMaterial;
                passData.source = cameraDepth;

                builder.UseTexture(cameraDepth, AccessFlags.Read);
                
                // 索引 0
                builder.SetRenderAttachment(destinationHandle, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }

        public void Dispose()
        {
            s_PreviousFrameDepthHandle?.Release();
            s_PreviousFrameDepthHandle = null;
            CoreUtils.Destroy(copyMaterial);
        }
    }
}