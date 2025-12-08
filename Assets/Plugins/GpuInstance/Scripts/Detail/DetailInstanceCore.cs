using UnityEngine;
using UnityEngine.Rendering;

public class DetailInstanceCore : System.IDisposable
{
    private FrustumCuller frustumCuller;

    private ComputeShader generateShader;
    private ComputeShader cachedCullingShader;

    private Mesh instanceMesh;
    private Material instanceMaterial;
    private MaterialPropertyBlock propertyBlock;

    private float _maxDist = 0;
    private float _fadeStart = 0;

    private ComputeBuffer instanceDataBuffer;
    private ComputeBuffer visibleInstancesBuffer;
    private ComputeBuffer visibleCountBuffer;
    private ComputeBuffer argsBuffer;

    private ComputeBuffer hzbVisibleBuffer;
    private ComputeBuffer hzbArgsBuffer;
    private ComputeBuffer hzbDebugBuffer;
    private ComputeBuffer hzbDispatchArgsBuffer;

    // 调试相关
    private ComputeBuffer debugCountersBuffer;
    private static readonly int DebugEnabledId = Shader.PropertyToID("_DebugEnabled");
    private static readonly int DebugCountersId = Shader.PropertyToID("_DebugCounters");
    public uint[] debugCounters = new uint[5]; // [0]=总数, [1]=frustum后, [2]=HZB后, [3]=tris, [4]=verts
    private uint meshTriCount = 0;
    private uint meshVertCount = 0;
    
    public int generatedCount = 0;
    private bool isInitialized = false;

    private bool needStatsUpdate = false;
    private float statsUpdateTimer = 0f;
    private const float STATS_UPDATE_INTERVAL = 0.5f;
    public int VisibleCount { get; private set; } = 0;

    private static readonly int TransformBufferId = Shader.PropertyToID("_TransformBuffer");
    private static readonly int VisibleInstancesBufferId = Shader.PropertyToID("_VisibleInstancesBuffer");
    private static readonly int UseCullingId = Shader.PropertyToID("_UseCulling");

    private static readonly int CameraPosId = Shader.PropertyToID("_CameraPos");
    private static readonly int MaxDistanceId = Shader.PropertyToID("_MaxDistance");
    private static readonly int FadeStartDistanceId = Shader.PropertyToID("_FadeStartDistance");
    
    private static readonly int DensityMapId = Shader.PropertyToID("_DensityMap");
    private static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");
    private static readonly int TerrainPositionId = Shader.PropertyToID("_TerrainPosition");
    private static readonly int TerrainSizeId = Shader.PropertyToID("_TerrainSize");
    private static readonly int DensityMapSizeId = Shader.PropertyToID("_DensityMapSize");
    private static readonly int SizeRangeId = Shader.PropertyToID("_SizeRange");
    private static readonly int HeightRangeId = Shader.PropertyToID("_HeightRange");
    private static readonly int NoiseSpreadId = Shader.PropertyToID("_NoiseSpread");
    private static readonly int DensityMultiplierId = Shader.PropertyToID("_DensityMultiplier");
    private static readonly int SeedId = Shader.PropertyToID("_Seed");
    private static readonly int AlignToGroundId = Shader.PropertyToID("_AlignToGround");
    private static readonly int HeightMapSizeId = Shader.PropertyToID("_HeightMapSize");
    private static readonly int LocalBoundsCenterId = Shader.PropertyToID("_LocalBoundsCenter");
    private static readonly int LocalBoundsExtentsId = Shader.PropertyToID("_LocalBoundsExtents");
    private static readonly int VoteBufferId = Shader.PropertyToID("_VoteBuffer");
    private static readonly int ResultBufferId = Shader.PropertyToID("_ResultBuffer");

    public bool IsInitialized => isInitialized;

    public void Initialize(
        Texture2D densityMap, Texture2D heightMap, DetailMetadata metadata,
        Vector3 terrainPos, Vector3 terrainSize, float grassDensity, float maxDist, float fadeStart,
        GameObject prefab, Shader instancedShader, ComputeShader genShader,
        ComputeShader cullShader, ComputeShader hzbGen, ComputeShader hzbCull)
    {
        Dispose();

        if (densityMap == null || prefab == null || genShader == null)
        {
            Debug.LogError("DetailInstanceCore: Missing resources.");
            return;
        }
        
        generateShader = genShader;
        cachedCullingShader = cullShader;
        _maxDist = maxDist;
        _fadeStart = fadeStart;

        instanceMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        Material originalMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;

        // 计算mesh的tris和verts
        meshTriCount = instanceMesh.GetIndexCount(0) / 3;
        meshVertCount = (uint)instanceMesh.vertexCount;

        instanceMaterial = CreateInstancedMaterial(originalMaterial, instancedShader);
        propertyBlock = new MaterialPropertyBlock();

        GenerateInstances(densityMap, heightMap, metadata, terrainPos, terrainSize, grassDensity);

        if (generatedCount == 0)
        {
            isInitialized = true;
            Debug.Log($"[DetailInstanceCore] Initialized empty layer (0 instances).");
            return;
        }

        if (cullShader != null)
        {
            frustumCuller = new FrustumCuller();
            frustumCuller.Initialize(cullShader);
            CreateCullingBuffers();
        }

        CreateHZBBuffers();

        // 创建调试buffer
        debugCountersBuffer = new ComputeBuffer(5, sizeof(uint));

        isInitialized = true;
        Debug.Log($"[DetailInstanceCore] Initialized. Count: {generatedCount}");
    }

    private void GenerateInstances(Texture2D densityMap, Texture2D heightMap, DetailMetadata meta, Vector3 tPos,
        Vector3 tSize, float grassDensity)
    {
        CommandBuffer initCmd = new CommandBuffer();
        initCmd.name = "DetailInstanceCore_Init";
        
        int kernelVote = generateShader.FindKernel("VoteInstances");
        int kernelGen = generateShader.FindKernel("GenerateGrass");

        initCmd.SetComputeTextureParam(generateShader, kernelVote, DensityMapId, densityMap);
        initCmd.SetComputeTextureParam(generateShader, kernelVote, HeightMapId, heightMap);
        initCmd.SetComputeVectorParam(generateShader, TerrainPositionId, tPos);
        initCmd.SetComputeVectorParam(generateShader, TerrainSizeId, tSize);
        initCmd.SetComputeVectorParam(generateShader, DensityMapSizeId, new Vector2(densityMap.width, densityMap.height));
        initCmd.SetComputeVectorParam(generateShader, SizeRangeId, new Vector2(meta.minWidth, meta.maxWidth));
        initCmd.SetComputeVectorParam(generateShader, HeightRangeId, new Vector2(meta.minHeight, meta.maxHeight));
        initCmd.SetComputeFloatParam(generateShader, NoiseSpreadId, meta.noiseSpread);
        initCmd.SetComputeFloatParam(generateShader, DensityMultiplierId, grassDensity);
        initCmd.SetComputeFloatParam(generateShader, SeedId, meta.noiseSeed);
        initCmd.SetComputeFloatParam(generateShader, AlignToGroundId, meta.alignToGround);
        initCmd.SetComputeVectorParam(generateShader, HeightMapSizeId, new Vector4(heightMap.width, heightMap.height, 1f / heightMap.width, 1f / heightMap.height));

        initCmd.SetComputeTextureParam(generateShader, kernelGen, DensityMapId, densityMap);
        initCmd.SetComputeTextureParam(generateShader, kernelGen, HeightMapId, heightMap);
        initCmd.SetComputeVectorParam(generateShader, TerrainPositionId, tPos);
        initCmd.SetComputeVectorParam(generateShader, TerrainSizeId, tSize);
        initCmd.SetComputeVectorParam(generateShader, DensityMapSizeId, new Vector2(densityMap.width, densityMap.height));
        initCmd.SetComputeVectorParam(generateShader, SizeRangeId, new Vector2(meta.minWidth, meta.maxWidth));
        initCmd.SetComputeVectorParam(generateShader, HeightRangeId, new Vector2(meta.minHeight, meta.maxHeight));
        initCmd.SetComputeFloatParam(generateShader, NoiseSpreadId, meta.noiseSpread);
        initCmd.SetComputeFloatParam(generateShader, DensityMultiplierId, grassDensity);
        initCmd.SetComputeFloatParam(generateShader, SeedId, meta.noiseSeed);
        initCmd.SetComputeFloatParam(generateShader, AlignToGroundId, meta.alignToGround);
        initCmd.SetComputeVectorParam(generateShader, HeightMapSizeId, new Vector4(heightMap.width, heightMap.height, 1f / heightMap.width, 1f / heightMap.height));
        
        Bounds b = instanceMesh.bounds;
        initCmd.SetComputeVectorParam(generateShader, LocalBoundsCenterId, new Vector4(b.center.x, b.center.y, b.center.z, 1));
        initCmd.SetComputeVectorParam(generateShader, LocalBoundsExtentsId, new Vector4(b.extents.x, b.extents.y, b.extents.z, 0));
        
        int groupX = Mathf.CeilToInt(densityMap.width / 8.0f);
        int groupY = Mathf.CeilToInt(densityMap.height / 8.0f);

        ComputeBuffer voteBuffer = new ComputeBuffer(1, sizeof(uint));
        voteBuffer.SetData(new uint[] { 0 });
        initCmd.SetComputeBufferParam(generateShader, kernelVote, VoteBufferId, voteBuffer);
        
        initCmd.BeginSample("GPU_Detail_Vote");
        initCmd.DispatchCompute(generateShader, kernelVote, groupX, groupY, 1);
        initCmd.EndSample("GPU_Detail_Vote");
        Graphics.ExecuteCommandBuffer(initCmd);
        
        uint[] voteResult = new uint[1];
        voteBuffer.GetData(voteResult);
        voteBuffer.Release();

        generatedCount = (int)voteResult[0];

        if (generatedCount == 0)
        {
            initCmd.Release();
            return;
        }

        instanceDataBuffer = new ComputeBuffer(generatedCount, 96, ComputeBufferType.Append);
        instanceDataBuffer.SetCounterValue(0);

        initCmd.Clear();
        initCmd.SetComputeBufferParam(generateShader, kernelGen, ResultBufferId, instanceDataBuffer);
        
        initCmd.BeginSample("GPU_Detail_Generate");
        initCmd.DispatchCompute(generateShader, kernelGen, groupX, groupY, 1);
        initCmd.EndSample("GPU_Detail_Generate");
        Graphics.ExecuteCommandBuffer(initCmd);
        
        initCmd.Release();
        
        float sizeMB = (generatedCount * 96.0f) / (1024 * 1024);
        Debug.Log($"[DetailInstanceCore] Optimized Buffer: {generatedCount} instances, Size: {sizeMB:F2} MB");
    }

    public void Render(CommandBuffer cmd, Camera camera, bool enableFrustum, bool enableHZB, float occlusionOffset, HZBOcclusionCuller sharedCuller, bool debugEnabled)
    {
        if (!isInitialized || generatedCount == 0 || cmd == null) return;

        bool useFrustum = enableFrustum && frustumCuller != null && frustumCuller.IsInitialized;
        bool useHZB = useFrustum && enableHZB && sharedCuller != null && sharedCuller.IsInitialized;

        ComputeBuffer finalVisibleBuffer = null;
        ComputeBuffer finalArgsBuffer = null;

        bool shouldUpdateStats = UpdateStatsTimer();
        if (shouldUpdateStats) VisibleCount = 0;

        // 清空调试计数器
        if (debugEnabled)
        {
            ClearDebugCounters(cmd);
        }

        if (useFrustum)
        {
            visibleInstancesBuffer.SetCounterValue(0);
            if (useHZB) hzbVisibleBuffer.SetCounterValue(0);

            if (cachedCullingShader != null)
            {
                cmd.SetComputeVectorParam(cachedCullingShader, CameraPosId, camera.transform.position);
                cmd.SetComputeFloatParam(cachedCullingShader, MaxDistanceId, _maxDist);
                cmd.SetComputeFloatParam(cachedCullingShader, FadeStartDistanceId, _fadeStart);
                
                // 绑定调试参数
                cmd.SetComputeIntParam(cachedCullingShader, DebugEnabledId, debugEnabled ? 1 : 0);
                cmd.SetComputeBufferParam(cachedCullingShader, frustumCuller.GetCullingKernel(), DebugCountersId, debugCountersBuffer);
            }

            frustumCuller.CullInstances(cmd, camera,
                instanceDataBuffer,
                visibleInstancesBuffer,
                visibleCountBuffer,
                instanceMesh.bounds.size,
                generatedCount);

            if (useHZB)
            {
                sharedCuller.SetOcclusionOffset(cmd, occlusionOffset);

// 总是绑定调试buffer（避免missing UAV错误），通过_DebugEnabled控制是否统计
                int hzbKernel = sharedCuller.GetHZBCullingKernel();
                ComputeShader hzbShader = sharedCuller.GetHZBCullingShader();
                cmd.SetComputeIntParam(hzbShader, DebugEnabledId, debugEnabled ? 1 : 0);
                cmd.SetComputeBufferParam(hzbShader, hzbKernel, DebugCountersId, debugCountersBuffer);

                sharedCuller.CullInstancesIndirect(
                    cmd,
                    camera,
                    instanceDataBuffer,
                    visibleInstancesBuffer,
                    visibleCountBuffer,
                    hzbVisibleBuffer,
                    hzbDebugBuffer,
                    hzbDispatchArgsBuffer
                );

                cmd.CopyCounterValue(hzbVisibleBuffer, hzbArgsBuffer, sizeof(uint));
                finalVisibleBuffer = hzbVisibleBuffer;
                finalArgsBuffer = hzbArgsBuffer;
            }
            else
            {
                cmd.CopyCounterValue(visibleInstancesBuffer, argsBuffer, sizeof(uint));
                finalVisibleBuffer = visibleInstancesBuffer;
                finalArgsBuffer = argsBuffer;
            }

            propertyBlock.SetInt(UseCullingId, 1);
            propertyBlock.SetBuffer(VisibleInstancesBufferId, finalVisibleBuffer);

            if (shouldUpdateStats && finalArgsBuffer != null)
            {
                uint[] argsData = new uint[5];
                finalArgsBuffer.GetData(argsData);
                VisibleCount = (int)argsData[1];
            }
        }
        else
        {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = instanceMesh.GetIndexCount(0);
            args[1] = (uint)generatedCount;
            args[2] = instanceMesh.GetIndexStart(0);
            args[3] = instanceMesh.GetBaseVertex(0);
            argsBuffer.SetData(args);

            finalArgsBuffer = argsBuffer;
            propertyBlock.SetInt(UseCullingId, 0);

            if (shouldUpdateStats) VisibleCount = generatedCount;
        }

        propertyBlock.SetFloat("_GrassShadowDistance", 50.0f);
        
        propertyBlock.SetBuffer(TransformBufferId, instanceDataBuffer);
        Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one * 100000f);

        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, worldBounds,
            finalArgsBuffer, 0, propertyBlock,
            ShadowCastingMode.On, true, 0, null, LightProbeUsage.BlendProbes);
    }

    private void ClearDebugCounters(CommandBuffer cmd)
    {
        uint[] zeros = new uint[5];
        debugCountersBuffer.SetData(zeros);
    }

    public void ReadDebugCounters()
    {
        if (debugCountersBuffer != null)
        {
            debugCountersBuffer.GetData(debugCounters);

            if (debugCounters[2] == 0)
            {
                debugCounters[2] = debugCounters[1];
            }
            
            // 计算tris和verts
            uint visibleCount = debugCounters[2];
            debugCounters[3] = visibleCount * meshTriCount;
            debugCounters[4] = visibleCount * meshVertCount;
        }
    }

    private void CreateCullingBuffers()
    {
        if (generatedCount == 0) return;
        visibleInstancesBuffer = new ComputeBuffer(generatedCount, sizeof(uint), ComputeBufferType.Append);
        visibleCountBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);
        argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = instanceMesh.GetIndexCount(0);
        args[2] = instanceMesh.GetIndexStart(0);
        args[3] = instanceMesh.GetBaseVertex(0);
        argsBuffer.SetData(args);
    }

    private void CreateHZBBuffers()
    {
        if (generatedCount == 0) return;
        hzbVisibleBuffer = new ComputeBuffer(generatedCount, sizeof(uint), ComputeBufferType.Append);
        hzbArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = instanceMesh.GetIndexCount(0);
        args[2] = instanceMesh.GetIndexStart(0);
        args[3] = instanceMesh.GetBaseVertex(0);
        hzbArgsBuffer.SetData(args);
        hzbDebugBuffer = new ComputeBuffer(generatedCount, sizeof(uint) * 2, ComputeBufferType.Append);
        hzbDispatchArgsBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    private Material CreateInstancedMaterial(Material original, Shader instancedShader)
    {
        if (original == null) return null;
        if (instancedShader == null)
        {
            if (!original.enableInstancing)
            {
                Material mat = new Material(original);
                mat.enableInstancing = true;
                return mat;
            }
            return original;
        }

        Material instanced = new Material(instancedShader);
        instanced.name = original.name + "_Instanced";

        if (original.HasProperty("_MainTex"))
            instanced.SetTexture("_BaseMap", original.GetTexture("_MainTex"));
        else if (original.HasProperty("_BaseMap"))
            instanced.SetTexture("_BaseMap", original.GetTexture("_BaseMap"));
        else if (original.HasProperty("_Diffuse"))
            instanced.SetTexture("_BaseMap", original.GetTexture("_Diffuse"));

        if (original.HasProperty("_Normal"))
            instanced.SetTexture("_NormalMap", original.GetTexture("_Normal"));
        else if (original.HasProperty("_BumpMap"))
            instanced.SetTexture("_NormalMap", original.GetTexture("_BumpMap"));

        if (original.HasProperty("_NormalPower"))
            instanced.SetFloat("_NormalScale", original.GetFloat("_NormalPower"));

        if (original.HasProperty("_Color"))
            instanced.SetColor("_BaseColor", original.GetColor("_Color"));
        else if (original.HasProperty("_BaseColor"))
            instanced.SetColor("_BaseColor", original.GetColor("_BaseColor"));
        else if (original.HasProperty("_MainColor"))
            instanced.SetColor("_BaseColor", original.GetColor("_MainColor"));
        else if (original.HasProperty("_Color01"))
            instanced.SetColor("_BaseColor", original.GetColor("_Color01"));

        if (original.HasProperty("_AlphaClipThreshold"))
            instanced.SetFloat("_AlphaClipThreshold", original.GetFloat("_AlphaClipThreshold"));
        else if (original.HasProperty("_Cutoff"))
            instanced.SetFloat("_AlphaClipThreshold", original.GetFloat("_Cutoff"));

        instanced.renderQueue = original.renderQueue;
        instanced.enableInstancing = true;
        return instanced;
    }
    
    private bool UpdateStatsTimer()
    {
        if (!needStatsUpdate) return false;
        statsUpdateTimer += Time.deltaTime;
        if (statsUpdateTimer >= STATS_UPDATE_INTERVAL) { statsUpdateTimer = 0f; return true; }
        return false;
    }

    public void Dispose()
    {
        instanceDataBuffer?.Release();
        visibleInstancesBuffer?.Release();
        visibleCountBuffer?.Release();
        argsBuffer?.Release();
        hzbVisibleBuffer?.Release();
        hzbArgsBuffer?.Release();
        hzbDebugBuffer?.Release();
        hzbDispatchArgsBuffer?.Release();
        debugCountersBuffer?.Release();
        frustumCuller?.Dispose();
        isInitialized = false;
        cachedCullingShader = null;
    }
}