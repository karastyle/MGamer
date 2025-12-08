using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class FrustumCuller
{
    private ComputeShader cullingShader;
    private int cullingKernel;
    private bool initialized = false;

    public bool IsInitialized => initialized;

    public bool Initialize(ComputeShader shader)
    {
        if (shader == null)
        {
            Debug.LogWarning("[FrustumCuller] Compute shader not provided");
            return false;
        }

        cullingShader = shader;
        cullingKernel = shader.FindKernel("CSCullInstances");
        initialized = true;
        
        Debug.Log("[FrustumCuller] Initialized successfully");
        return true;
    }

    public int GetCullingKernel()
    {
        return cullingKernel;
    }

    public void CullInstances(
        CommandBuffer cmd,
        Camera camera,
        ComputeBuffer instanceDataBuffer,
        ComputeBuffer outputBuffer,
        ComputeBuffer visibleCountBuffer,
        Vector3 boundsSize,
        int totalCount)
    {
        if (!initialized || camera == null || instanceDataBuffer == null || outputBuffer == null || cmd == null)
            return;

        outputBuffer.SetCounterValue(0);

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        Vector4[] planesVector = new Vector4[6];
        for (int i = 0; i < 6; i++)
        {
            planesVector[i] = new Vector4(
                frustumPlanes[i].normal.x,
                frustumPlanes[i].normal.y,
                frustumPlanes[i].normal.z,
                frustumPlanes[i].distance
            );
        }

        int frustumPlanesId = Shader.PropertyToID("_FrustumPlanes");
        int instanceCountId = Shader.PropertyToID("_InstanceCount");
        int boundsSizeId = Shader.PropertyToID("_BoundsSize");
        int instanceDataBufferId = Shader.PropertyToID("_InstanceDataBuffer");
        int visibleInstancesBufferId = Shader.PropertyToID("_VisibleInstancesBuffer");
        
        cmd.SetComputeVectorArrayParam(cullingShader, frustumPlanesId, planesVector);
        cmd.SetComputeIntParam(cullingShader, instanceCountId, totalCount);
        cmd.SetComputeVectorParam(cullingShader, boundsSizeId, boundsSize);
        cmd.SetComputeBufferParam(cullingShader, cullingKernel, instanceDataBufferId, instanceDataBuffer);
        cmd.SetComputeBufferParam(cullingShader, cullingKernel, visibleInstancesBufferId, outputBuffer);

        int threadGroups = Mathf.CeilToInt(totalCount / 64f);

        cmd.BeginSample("GPU_FrustumCuller_Dispatch");
        cmd.DispatchCompute(cullingShader, cullingKernel, threadGroups, 1, 1);
        cmd.EndSample("GPU_FrustumCuller_Dispatch");
        
        if (visibleCountBuffer != null)
        {
            cmd.CopyCounterValue(outputBuffer, visibleCountBuffer, 0);
        }
    }

    public void Dispose()
    {
        initialized = false;
    }
}