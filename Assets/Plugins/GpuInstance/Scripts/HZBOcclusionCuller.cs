using UnityEngine;
using UnityEngine.Rendering;

public class HZBOcclusionCuller
{
    private ComputeShader hzbGenerator;
    private int hzbGenerateKernel;

    private ComputeShader hzbCulling;
    private int hzbCullingKernel;

    private RenderTexture hzbTexture;
    private RenderTexture[] hzbMipTextures;
    private int hzbMaxMipLevel = 0;

    private bool useReversedZ;

    private int hzbSetupArgsKernel;

    public bool IsInitialized { get; private set; }
    public RenderTexture HZBTexture => hzbTexture;
    public int MaxMipLevel => hzbMaxMipLevel;

    public bool Initialize(ComputeShader generator, ComputeShader culler)
    {
        // 🟢 [修改] 只检查 generator，允许 culler 为空
        if (generator == null)
        {
            Debug.LogWarning("[HZBOcclusionCuller] Generator shader not provided");
            return false;
        }

        hzbGenerator = generator;
        hzbCulling = culler;

        hzbGenerateKernel = generator.FindKernel("BuildHZB");
    
        // 🟢 [修改] 只有当 culler 存在时才查找剔除 Kernel
        if (hzbCulling != null)
        {
            hzbCullingKernel = culler.FindKernel("CullInstancesHZB");
            hzbSetupArgsKernel = culler.FindKernel("SetupIndirectArgs");
        }

        useReversedZ = SystemInfo.usesReversedZBuffer;

        IsInitialized = true;
    
        // 🟢 [优化日志]
        string mode = (hzbCulling != null) ? "Full Mode" : "Generation Only Mode";
        Debug.Log($"[HZBOcclusionCuller] Initialized ({mode}) - Reversed-Z: {useReversedZ}");
    
        return true;
    }

    public int GetHZBCullingKernel()
    {
        return hzbCullingKernel;
    }

    public ComputeShader GetHZBCullingShader()
    {
        return hzbCulling;
    }

    // 🟢 [修改] 在执行剔除方法前增加空检查
    public void SetOcclusionOffset(CommandBuffer cmd, float offset)
    {
        if (hzbCulling != null && cmd != null)
        {
            int occlusionOffsetId = Shader.PropertyToID("_OcclusionOffset");
            cmd.SetComputeFloatParam(hzbCulling, occlusionOffsetId, offset);
        }
    }
    
    public bool GenerateHZB(CommandBuffer cmd, Camera camera)
    {
        if (!IsInitialized || camera == null || cmd == null)
            return false;

        RenderTexture depthTexture = CopyDepthFeature.CopyDepthPass.PreviousFrameDepth;

        if (depthTexture == null)
        {
            Debug.LogWarning("[HZB] Previous frame depth not available yet");
            return false;
        }

        int width = depthTexture.width;
        int height = depthTexture.height;

        hzbMaxMipLevel = (int)Mathf.Floor(Mathf.Log(Mathf.Max(width, height), 2));

        if (hzbTexture == null || hzbTexture.width != width || hzbTexture.height != height)
        {
            ReleaseHZBTextures();

            RenderTextureDescriptor desc = new RenderTextureDescriptor(
                width, height, RenderTextureFormat.RFloat, 0);
            desc.enableRandomWrite = true;
            desc.useMipMap = true;
            desc.autoGenerateMips = false;
            desc.mipCount = hzbMaxMipLevel + 1;

            hzbTexture = new RenderTexture(desc);
            hzbTexture.filterMode = FilterMode.Point;
            hzbTexture.wrapMode = TextureWrapMode.Clamp;
            hzbTexture.Create();

            Debug.Log($"[HZB] Created HZB: {width}x{height}, mips: {hzbMaxMipLevel + 1}");
        }

        if (hzbMipTextures == null || hzbMipTextures.Length != hzbMaxMipLevel + 1)
        {
            if (hzbMipTextures != null)
            {
                foreach (var rt in hzbMipTextures)
                    if (rt != null) rt.Release();
            }
            hzbMipTextures = new RenderTexture[hzbMaxMipLevel + 1];
        }

        // 🟢 [修复] 使用 cmd.SetCompute...
        int useReversedZId = Shader.PropertyToID("_UseReversedZ");
        cmd.SetComputeIntParam(hzbGenerator, useReversedZId, useReversedZ ? 1 : 0);

        if (hzbMipTextures[0] == null || 
            hzbMipTextures[0].width != width || 
            hzbMipTextures[0].height != height)
        {
            if (hzbMipTextures[0] != null)
                hzbMipTextures[0].Release();

            RenderTextureDescriptor desc = new RenderTextureDescriptor(
                width, height, RenderTextureFormat.RFloat, 0);
            desc.enableRandomWrite = true;

            hzbMipTextures[0] = new RenderTexture(desc);
            hzbMipTextures[0].filterMode = FilterMode.Point;
            hzbMipTextures[0].Create();
        }

        Graphics.Blit(depthTexture, hzbMipTextures[0]);
        Graphics.CopyTexture(hzbMipTextures[0], 0, 0, hzbTexture, 0, 0);

        cmd.Blit(depthTexture, hzbMipTextures[0]);
    
        // 🟢 [关键修复] 将 Graphics.CopyTexture 改为 cmd.CopyTexture
        cmd.CopyTexture(hzbMipTextures[0], 0, 0, hzbTexture, 0, 0);
        int sourceDepthId = Shader.PropertyToID("_SourceDepth");
        int sourceSizeId = Shader.PropertyToID("_SourceSize");
        int targetMipId = Shader.PropertyToID("_TargetMip");
        int targetSizeId = Shader.PropertyToID("_TargetSize");
        
        for (int mip = 1; mip <= hzbMaxMipLevel; mip++)
        {
            int mipWidth = Mathf.Max(1, width >> mip);
            int mipHeight = Mathf.Max(1, height >> mip);

            if (hzbMipTextures[mip] == null ||
                hzbMipTextures[mip].width != mipWidth ||
                hzbMipTextures[mip].height != mipHeight)
            {
                if (hzbMipTextures[mip] != null)
                    hzbMipTextures[mip].Release();

                RenderTextureDescriptor desc = new RenderTextureDescriptor(
                    mipWidth, mipHeight, RenderTextureFormat.RFloat, 0);
                desc.enableRandomWrite = true;

                hzbMipTextures[mip] = new RenderTexture(desc);
                hzbMipTextures[mip].filterMode = FilterMode.Point;
                hzbMipTextures[mip].Create();
            }

            cmd.SetComputeTextureParam(hzbGenerator, hzbGenerateKernel, sourceDepthId, hzbMipTextures[mip - 1]);
            int prevWidth = Mathf.Max(1, width >> (mip - 1));
            int prevHeight = Mathf.Max(1, height >> (mip - 1));
            cmd.SetComputeIntParams(hzbGenerator, sourceSizeId, new int[] { prevWidth, prevHeight });

            cmd.SetComputeTextureParam(hzbGenerator, hzbGenerateKernel, targetMipId, hzbMipTextures[mip]);
            cmd.SetComputeIntParams(hzbGenerator, targetSizeId, new int[] { mipWidth, mipHeight });

            int threadGroupsX = Mathf.CeilToInt(mipWidth / 8f);
            int threadGroupsY = Mathf.CeilToInt(mipHeight / 8f);

            cmd.BeginSample("GPU_HZB_Mip");
            cmd.DispatchCompute(hzbGenerator, hzbGenerateKernel, threadGroupsX, threadGroupsY, 1);
            cmd.EndSample("GPU_HZB_Mip");
            
            // 🟢 [关键修复] 改为 cmd.CopyTexture
            cmd.CopyTexture(hzbMipTextures[mip], 0, 0, hzbTexture, 0, mip);
        }

        return true;
    }
    
    public void CullInstancesIndirect(
        CommandBuffer cmd,
        Camera camera,
        ComputeBuffer instanceDataBuffer,
        ComputeBuffer inputBuffer,
        ComputeBuffer inputCountBuffer,
        ComputeBuffer outputBuffer,
        ComputeBuffer debugBuffer,
        ComputeBuffer indirectArgsBuffer)
    {
        // 🟢 [新增] 如果没有剔除Shader，直接返回
        if (!IsInitialized || hzbTexture == null || cmd == null || hzbCulling == null) return;

        outputBuffer.SetCounterValue(0);

        int countBufferId = Shader.PropertyToID("_CountBuffer");
        int indirectArgsBufferId = Shader.PropertyToID("_IndirectArgsBuffer");
        
        cmd.SetComputeBufferParam(hzbCulling, hzbSetupArgsKernel, countBufferId, inputCountBuffer);
        cmd.SetComputeBufferParam(hzbCulling, hzbSetupArgsKernel, indirectArgsBufferId, indirectArgsBuffer);

        cmd.BeginSample("GPU_HZB_Count");
        cmd.DispatchCompute(hzbCulling, hzbSetupArgsKernel, 1, 1, 1);
        cmd.EndSample("GPU_HZB_Count");
        
        Matrix4x4 vp = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix;
        
        int vpMatrixId = Shader.PropertyToID("_ViewProjectionMatrix");
        int cameraWorldPosId = Shader.PropertyToID("_CameraWorldPos");
        int hzbSizeId = Shader.PropertyToID("_HZBSize");
        int maxMipLevelId = Shader.PropertyToID("_MaxMipLevel");
        int useReversedZId = Shader.PropertyToID("_UseReversedZ");
        
        cmd.SetComputeMatrixParam(hzbCulling, vpMatrixId, vp);
        cmd.SetComputeVectorParam(hzbCulling, cameraWorldPosId, camera.transform.position);
        cmd.SetComputeVectorParam(hzbCulling, hzbSizeId, new Vector2(hzbTexture.width, hzbTexture.height));
        cmd.SetComputeIntParam(hzbCulling, maxMipLevelId, hzbMaxMipLevel);
        cmd.SetComputeIntParam(hzbCulling, useReversedZId, useReversedZ ? 1 : 0);
        
        int kernel = hzbCullingKernel;
    
        int hzbTextureId = Shader.PropertyToID("_HZBTexture");
        int instanceDataBufferId = Shader.PropertyToID("_InstanceDataBuffer");
        int frustumVisibleBufferId = Shader.PropertyToID("_FrustumVisibleBuffer");
        int hzbVisibleBufferId = Shader.PropertyToID("_HZBVisibleBuffer");
        int inputCountBufferId = Shader.PropertyToID("_InputCountBuffer");
        
        cmd.SetComputeTextureParam(hzbCulling, kernel, hzbTextureId, hzbTexture);
        cmd.SetComputeBufferParam(hzbCulling, kernel, instanceDataBufferId, instanceDataBuffer);
        cmd.SetComputeBufferParam(hzbCulling, kernel, frustumVisibleBufferId, inputBuffer);
        cmd.SetComputeBufferParam(hzbCulling, kernel, hzbVisibleBufferId, outputBuffer);
        cmd.SetComputeBufferParam(hzbCulling, kernel, inputCountBufferId, inputCountBuffer);

        if (debugBuffer != null)
        {
            int hzbDebugBufferId = Shader.PropertyToID("_HZBDebugBuffer");
            cmd.SetComputeBufferParam(hzbCulling, kernel, hzbDebugBufferId, debugBuffer);
        }
        
        cmd.BeginSample("GPU_HZB_Cull");
        cmd.DispatchCompute(hzbCulling, kernel, indirectArgsBuffer, 0);
        cmd.EndSample("GPU_HZB_Cull");
    }

    private void ReleaseHZBTextures()
    {
        if (hzbTexture != null)
        {
            hzbTexture.Release();
            hzbTexture = null;
        }

        if (hzbMipTextures != null)
        {
            foreach (var rt in hzbMipTextures)
                if (rt != null) rt.Release();
            hzbMipTextures = null;
        }
    }

    public void Dispose()
    {
        ReleaseHZBTextures();
        IsInitialized = false;
        Debug.Log("[HZBOcclusionCuller] Disposed");
    }
}