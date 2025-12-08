using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// ✅ Segmented Big Buffer方案：真正的GPU Driven Pipeline
// Frustum + LOD + HZB 合并在1个Mega Kernel中完成
// 使用InterlockedAdd直接写入全局大buffer的对应segment
public class TreeInstanceCore : System.IDisposable
{
    // 1. 定义 Property ID (放在类的头部静态字段里)
    private static readonly int TotalDrawCallsId = Shader.PropertyToID("_TotalDrawCalls");

    private ComputeShader megaCullShader;
    private HZBOcclusionCuller hzbCuller;
    private Shader instancedShader;

    private int totalInstances = 0;

// 在类变量中添加
    private ComputeBuffer segmentLookupBuffer; // 新增：Prototype+LOD 到 SegmentIndex 的映射
    private static readonly int SegmentLookupId = Shader.PropertyToID("_SegmentLookup");

    // ✅ Segmented Big Buffer核心数据
    private ComputeBuffer globalInstanceDataBuffer; // 所有树的InstanceData
    private ComputeBuffer globalVisibleIndexBuffer; // 大buffer：存储可见实例索引
    private ComputeBuffer globalArgsBuffer; // 大buffer：所有DrawIndirect参数
    private ComputeBuffer segmentInfoBuffer; // 每个segment的信息
    private ComputeBuffer segmentOffsetsBuffer; // 用于shader快速查找

    // 调试相关
    private ComputeBuffer debugCountersBuffer;
    private static readonly int DebugEnabledId = Shader.PropertyToID("_DebugEnabled");
    private static readonly int DebugCountersId = Shader.PropertyToID("_DebugCounters");
    public uint[] debugCounters = new uint[6]; // [0]=总数, [1]=frustum后, [2]=LOD后, [3]=HZB后, [4]=tris, [5]=verts

    private int megaCullKernel;
    private int totalSegments;

    private static readonly int TotalSegmentsId = Shader.PropertyToID("_TotalSegments"); // 🟢 [新增]

// ✅ Segment信息 (必须与 Shader 对齐)
    private struct SegmentInfo
    {
        public uint indexOffset;
        public uint argsOffset;
        public uint maxCapacity;
        public uint prototypeIndex;
        public uint lodIndex;
        public uint subMeshCount;
        public uint triCount;
        public uint vertCount;
        public static int Size() => sizeof(uint) * 8; // 8个uint
    }

    // ✅ InstanceData (GPU)
    private struct InstanceData
    {
        public Matrix4x4 transformMatrix;
        public Vector4 boundsCenter;
        public Vector4 boundsExtents;
        public uint prototypeID; // 标识是哪种树
        public uint padding1;
        public uint padding2;
        public uint padding3;
        public static int Size() => 96 + 16; // 对齐到16字节
    }

    private class LODLevelData
    {
        public int lodIndex;
        public Mesh mesh;
        public Material[] originalMaterials;
        public Material[] instancedMaterials;

        public int segmentIndex; // 在segmentInfoBuffer中的索引
        public uint indexOffset; // 在globalVisibleIndexBuffer中的起始位置
        public uint argsOffset; // 在globalArgsBuffer中的位置
    }

    private class PrototypeRenderData
    {
        public int prototypeIndex;
        public GameObject prefab;
        public Vector3 boundsSize;
        public List<Matrix4x4> transforms = new List<Matrix4x4>();
        public List<LODLevelData> lodLevels = new List<LODLevelData>();
        public Vector4 lodDistances;
    }

    private List<PrototypeRenderData> prototypeRenderDatas = new List<PrototypeRenderData>();
    private TreeInstancesData treeData;
    private GameObject[] prototypePrefabs;
    private Bounds worldBounds;
    private MaterialPropertyBlock propertyBlock;

    // Shader Property IDs
    private static readonly int GlobalInstancesId = Shader.PropertyToID("_GlobalInstances");
    private static readonly int GlobalVisibleIndicesId = Shader.PropertyToID("_GlobalVisibleIndices");
    private static readonly int GlobalArgsId = Shader.PropertyToID("_GlobalArgs");
    private static readonly int SegmentInfoId = Shader.PropertyToID("_SegmentInfo");
    private static readonly int FrustumPlanesId = Shader.PropertyToID("_FrustumPlanes");
    private static readonly int CameraPosId = Shader.PropertyToID("_CameraPos");
    private static readonly int TotalInstanceCountId = Shader.PropertyToID("_TotalInstanceCount");
    private static readonly int UseHZBId = Shader.PropertyToID("_UseHZB");
    private static readonly int HZBTextureId = Shader.PropertyToID("_HZBTexture");
    private static readonly int ViewProjectionMatrixId = Shader.PropertyToID("_ViewProjectionMatrix");
    private static readonly int HZBSizeId = Shader.PropertyToID("_HZBSize");
    private static readonly int MaxMipLevelId = Shader.PropertyToID("_MaxMipLevel");
    private static readonly int UseReversedZId = Shader.PropertyToID("_UseReversedZ");

    private static readonly int GlobalIndexOffsetId = Shader.PropertyToID("_GlobalIndexOffset");
    private static readonly int TransformBufferId = Shader.PropertyToID("_TransformBuffer");

    public bool IsInitialized { get; private set; }

    public void Initialize(
        TreeInstancesData treeData,
        GameObject[] prototypePrefabs,
        Shader instancedShader,
        ComputeShader megaCullShader,
        ComputeShader hzbGenShader = null)
    {
        Dispose();

        this.instancedShader = instancedShader;
        this.treeData = treeData;
        this.prototypePrefabs = prototypePrefabs;
        this.megaCullShader = megaCullShader;

        if (treeData == null || treeData.instances == null || prototypePrefabs == null) return;

        propertyBlock = new MaterialPropertyBlock();

        BuildRenderData();
        CalculateWorldBounds();

        if (hzbGenShader != null)
        {
            hzbCuller = new HZBOcclusionCuller();
            hzbCuller.Initialize(hzbGenShader, null);
        }

        // ✅ 构建Segmented Big Buffer
        BuildSegmentedBuffers();

        // 创建调试buffer（6个uint：总数、frustum后、LOD后、HZB后、tris、verts）
        debugCountersBuffer = new ComputeBuffer(6, sizeof(uint));

        if (megaCullShader != null)
        {
            megaCullKernel = megaCullShader.FindKernel("MegaCullKernel");
        }

        IsInitialized = true;
        Debug.Log(
            $"[TreeInstanceCore] Initialized with Segmented Big Buffer: {totalInstances} instances, {totalSegments} segments");
    }

    public void Render(Camera camera, CommandBuffer cmd, bool useHZB, bool debugEnabled)
    {
        // 🟢 [修改] 增加 totalInstances == 0 的检查
        // 如果没有树，或者是 null，直接跳过，不要尝试 Dispatch
        if (!IsInitialized || cmd == null || totalInstances == 0)
            return;

        // 双重保险：检查核心 Buffer 是否为空
        if (globalArgsBuffer == null || segmentLookupBuffer == null)
            return;

        if (camera == null) camera = Camera.main;

        RenderWithMegaKernel(camera, cmd, useHZB, debugEnabled);
    }

    // ✅ Mega Kernel渲染路径：1次dispatch完成所有工作
// TreeInstanceCore.cs

    private void RenderWithMegaKernel(Camera camera, CommandBuffer cmd, bool useHZB, bool debugEnabled)
    {
        // 1. 重置Args buffer的InstanceCount
        ClearArgCounters(cmd);

        // 2. 如果开启调试，清空调试计数器
        if (debugEnabled)
        {
            ClearDebugCounters(cmd);
        }

        if (useHZB)
        {
            hzbCuller.GenerateHZB(cmd, camera);
        }

        // 3. 执行Mega Kernel（传入实际状态）
        DispatchMegaKernel(camera, cmd, useHZB, debugEnabled);

        // 🟢 [新增] 4. 同步 Submesh 计数器
        // 这一步非常关键！它把树干的数量复制给树叶
        DispatchPropagateCounts(cmd);

        DrawAllPrototypes(cmd);
    }

    // 清空调试计数器
    private void ClearDebugCounters(CommandBuffer cmd)
    {
        uint[] zeros = new uint[6];
        debugCountersBuffer.SetData(zeros);
    }

    // 读取调试计数器
    public void ReadDebugCounters()
    {
        if (debugCountersBuffer != null)
        {
            debugCountersBuffer.GetData(debugCounters);
        }
    }

    // 🟢 [新增函数] 调度同步 Kernel
    private void DispatchPropagateCounts(CommandBuffer cmd)
    {
        int kernel = megaCullShader.FindKernel("PropagateSubmeshCounts");

        cmd.SetComputeBufferParam(megaCullShader, kernel, SegmentInfoId, segmentInfoBuffer);
        cmd.SetComputeBufferParam(megaCullShader, kernel, GlobalArgsId, globalArgsBuffer);
        cmd.SetComputeIntParam(megaCullShader, TotalSegmentsId, totalSegments);

        int groups = Mathf.CeilToInt(totalSegments / 64f);
        cmd.BeginSample("GPU_MegaCull_PropagateSubmeshCounts");
        cmd.DispatchCompute(megaCullShader, kernel, groups, 1, 1);
        cmd.EndSample("GPU_MegaCull_PropagateSubmeshCounts");
    }
    

    private void ClearArgCounters(CommandBuffer cmd)
    {
        ComputeShader resetShader = megaCullShader;
        // 使用新的 Kernel 名字
        int kernel = resetShader.FindKernel("ClearArgCounters");

        // 计算总共有多少个 DrawCall (每个 DrawCall 5 个 uint)
        int totalDrawCalls = globalArgsBuffer.count / 5;

        // 绑定 Buffer
        cmd.SetComputeBufferParam(resetShader, kernel, GlobalArgsId, globalArgsBuffer);

        // 🟢 [修复] 传入刚才在 Shader 里定义的变量
        cmd.SetComputeIntParam(resetShader, TotalDrawCallsId, totalDrawCalls);

        // Dispatch
        int threadGroups = Mathf.CeilToInt(totalDrawCalls / 64f);
        cmd.BeginSample("GPU_MegaCull_ClearArgCounters");
        cmd.DispatchCompute(resetShader, kernel, threadGroups, 1, 1);
        cmd.EndSample("GPU_MegaCull_ClearArgCounters");
    }

    // ✅ 执行Mega Kernel
    private void DispatchMegaKernel(Camera camera, CommandBuffer cmd, bool useHZB, bool debugEnabled)
    {
        // 设置Frustum平面
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        Vector4[] planesVec = new Vector4[6];
        for (int i = 0; i < 6; i++)
        {
            planesVec[i] = new Vector4(
                planes[i].normal.x,
                planes[i].normal.y,
                planes[i].normal.z,
                planes[i].distance
            );
        }

        cmd.SetComputeVectorArrayParam(megaCullShader, FrustumPlanesId, planesVec);
        cmd.SetComputeVectorParam(megaCullShader, CameraPosId, camera.transform.position);
        cmd.SetComputeIntParam(megaCullShader, TotalInstanceCountId, totalInstances);

        cmd.SetComputeIntParam(megaCullShader, UseHZBId, useHZB?1:0);

        Matrix4x4 vp = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix;
        cmd.SetComputeMatrixParam(megaCullShader, ViewProjectionMatrixId, vp);
        // 绑定真正的 HZB 纹理
        cmd.SetComputeTextureParam(megaCullShader, megaCullKernel, HZBTextureId, hzbCuller.HZBTexture);

        if (useHZB)
        {
            // 🟢 [修复点] 此时访问 .width 是安全的，因为上面检查了 != null
            cmd.SetComputeVectorParam(megaCullShader, HZBSizeId,
                new Vector2(hzbCuller.HZBTexture.width, hzbCuller.HZBTexture.height));
        }
        
        cmd.SetComputeIntParam(megaCullShader, MaxMipLevelId, hzbCuller.MaxMipLevel);
        cmd.SetComputeIntParam(megaCullShader, UseReversedZId, SystemInfo.usesReversedZBuffer ? 1 : 0);

        // 调试参数
        cmd.SetComputeIntParam(megaCullShader, DebugEnabledId, debugEnabled ? 1 : 0);
        cmd.SetComputeBufferParam(megaCullShader, megaCullKernel, DebugCountersId, debugCountersBuffer);

        // 绑定buffers
        cmd.SetComputeBufferParam(megaCullShader, megaCullKernel, GlobalInstancesId, globalInstanceDataBuffer);
        cmd.SetComputeBufferParam(megaCullShader, megaCullKernel, GlobalVisibleIndicesId, globalVisibleIndexBuffer);
        cmd.SetComputeBufferParam(megaCullShader, megaCullKernel, GlobalArgsId, globalArgsBuffer);
        cmd.SetComputeBufferParam(megaCullShader, megaCullKernel, SegmentInfoId, segmentInfoBuffer);

        // Dispatch
        int threadGroups = Mathf.CeilToInt(totalInstances / 64f);

        // 🟢 [新增绑定]
        cmd.SetComputeBufferParam(megaCullShader, megaCullKernel, SegmentLookupId, segmentLookupBuffer);

        cmd.BeginSample("GPU_MegaCull_1Dispatch");
        cmd.DispatchCompute(megaCullShader, megaCullKernel, threadGroups, 1, 1);
        cmd.EndSample("GPU_MegaCull_1Dispatch");
    }

// 2. 修改 DrawAllPrototypes：改回 Graphics.DrawMeshInstancedIndirect
    private void DrawAllPrototypes(CommandBuffer cmd)
    {
        foreach (var renderData in prototypeRenderDatas)
        {
            foreach (var lod in renderData.lodLevels)
            {
                Material[] materials = lod.instancedMaterials ?? lod.originalMaterials;

                propertyBlock.SetBuffer(TransformBufferId, globalInstanceDataBuffer);
                propertyBlock.SetInt(GlobalIndexOffsetId, (int)lod.indexOffset);
                propertyBlock.SetFloat("_GrassShadowDistance", 50.0f);
                // 🟢 [关键修复 - 务必保留] 必须绑定这个 Buffer！Shader 里的 _GlobalVisibleIndices 靠它
                propertyBlock.SetBuffer(GlobalVisibleIndicesId, globalVisibleIndexBuffer);

                for (int i = 0; i < lod.mesh.subMeshCount; i++)
                {
                    if (i >= materials.Length || materials[i] == null) continue;

                    uint argsOffsetBytes = lod.argsOffset * sizeof(uint) + (uint)(i * 5 * sizeof(uint));

                    // 🟢 [修改] 智能阴影策略
                    // 策略：只有近处的树 (LOD 0) 投射阴影，或者 LOD 0 和 1。
                    // 远处的树通常不需要投射阴影（节省 ShadowMap 渲染开销）。
                    ShadowCastingMode shadowMode = ShadowCastingMode.Off;
                    bool receiveShadows = false;

                    // 这里的逻辑可以根据你的需求调整：
                    // 如果想要极致性能： lod.lodIndex == 0 (只有最近的树有阴影)
                    if (lod.lodIndex == 0 || lod.lodIndex == 1) 
                    {
                        shadowMode = ShadowCastingMode.On;
                        receiveShadows = true;
                    }
                    
                    Graphics.DrawMeshInstancedIndirect(
                        lod.mesh, i, materials[i], worldBounds,
                        globalArgsBuffer, (int)argsOffsetBytes,
                        propertyBlock, shadowMode, receiveShadows, 0, null, LightProbeUsage.BlendProbes
                    );
                }
            }
        }
    }

    // ✅ 构建Segmented Big Buffer
    private void BuildSegmentedBuffers()
    {
        if (prototypeRenderDatas.Count == 0) return;

        // 1. 构建全局InstanceData数组（添加prototypeID）
        List<InstanceData> globalInstances = new List<InstanceData>();
        List<SegmentInfo> segments = new List<SegmentInfo>();

        uint currentIndexOffset = 0;
        uint currentArgsOffset = 0;
        int segmentIndex = 0;

        foreach (var renderData in prototypeRenderDatas)
        {
            Mesh lod0Mesh = renderData.lodLevels.Count > 0 ? renderData.lodLevels[0].mesh : null;
            Vector3 localCenter = lod0Mesh != null ? lod0Mesh.bounds.center : Vector3.zero;
            Vector3 localExtents = lod0Mesh != null ? lod0Mesh.bounds.extents : Vector3.one * 0.5f;

            // 为该prototype的每个实例创建InstanceData
            foreach (var mat in renderData.transforms)
            {
                Vector3 worldCenter = mat.MultiplyPoint3x4(localCenter);
                Vector3 scale = new Vector3(
                    mat.GetColumn(0).magnitude,
                    mat.GetColumn(1).magnitude,
                    mat.GetColumn(2).magnitude
                );
                Vector3 worldExtents = Vector3.Scale(localExtents, scale);

                globalInstances.Add(new InstanceData
                {
                    transformMatrix = mat,
                    boundsCenter = new Vector4(worldCenter.x, worldCenter.y, worldCenter.z, 1.0f),
                    boundsExtents = new Vector4(worldExtents.x, worldExtents.y, worldExtents.z, 0.0f),
                    prototypeID = (uint)renderData.prototypeIndex,
                    padding1 = 0,
                    padding2 = 0,
                    padding3 = 0
                });
            }

            // 为该prototype的每个LOD创建segment
            foreach (var lod in renderData.lodLevels)
            {
                uint maxCapacity = (uint)renderData.transforms.Count;
                // 🟢 [修改] 记录 Submesh 数量
                uint subMeshCount = (uint)lod.mesh.subMeshCount;
                
                // 计算该LOD的总三角形和顶点数（所有submesh累加）
                uint triCount = 0;
                uint vertCount = (uint)lod.mesh.vertexCount;
                for (int i = 0; i < lod.mesh.subMeshCount; i++)
                {
                    triCount += lod.mesh.GetIndexCount(i) / 3;
                }
                
                SegmentInfo seg = new SegmentInfo
                {
                    indexOffset = currentIndexOffset,
                    argsOffset = currentArgsOffset,
                    maxCapacity = maxCapacity,
                    prototypeIndex = (uint)renderData.prototypeIndex,
                    lodIndex = (uint)lod.lodIndex,
                    subMeshCount = subMeshCount, // 🟢 [赋值]
                    triCount = triCount,
                    vertCount = vertCount
                };
                segments.Add(seg);

                lod.segmentIndex = segmentIndex;
                lod.indexOffset = currentIndexOffset;
                lod.argsOffset = currentArgsOffset;

                // 每个submesh需要5个uint的args
                uint argsPerLOD = (uint)(lod.mesh.subMeshCount * 5);

                currentIndexOffset += maxCapacity;
                currentArgsOffset += argsPerLOD;
                segmentIndex++;
            }
        }

        totalSegments = segments.Count;

        // 🟢 [新增] 确保 prototypePrefabs 有效，防止 lookupTable 计算出错
        if (prototypePrefabs == null || prototypePrefabs.Length == 0)
        {
            Debug.LogError("Prototype Prefabs is empty!");
            return;
        }

        // 🟢 [新增逻辑] 构建查找表
        // 假设最大支持 3 个 LOD，容量 = Prototype数量 * 3
        // 这里的 3 必须和 Shader 里的 LOD_DISTANCES 数组长度一致
        int maxLods = 3;
        int maxPrototypes = prototypePrefabs.Length; // 确保取最大可能的ID
        int[] lookupTable = new int[maxPrototypes * maxLods];

        // 初始化为 -1 (无效)
        for (int i = 0; i < lookupTable.Length; i++) lookupTable[i] = -1;

        // 填充表
        foreach (var renderData in prototypeRenderDatas)
        {
            foreach (var lod in renderData.lodLevels)
            {
                // Key: ProtoID * 3 + LOD
                int key = renderData.prototypeIndex * maxLods + lod.lodIndex;
                // Value: 实际的 SegmentIndex
                if (key < lookupTable.Length)
                {
                    lookupTable[key] = lod.segmentIndex;
                }
            }
        }

        // 创建并上传 Buffer
        segmentLookupBuffer = new ComputeBuffer(lookupTable.Length, sizeof(int));
        segmentLookupBuffer.SetData(lookupTable);

        // 2. 创建buffers
        if (globalInstances.Count > 0)
        {
            globalInstanceDataBuffer = new ComputeBuffer(globalInstances.Count, InstanceData.Size());
            globalInstanceDataBuffer.SetData(globalInstances.ToArray());

            // 大buffer：存储可见实例索引
            globalVisibleIndexBuffer = new ComputeBuffer((int)currentIndexOffset, sizeof(uint));

            // Args buffer：存储所有DrawIndirect参数
            globalArgsBuffer =
                new ComputeBuffer((int)currentArgsOffset, sizeof(uint), ComputeBufferType.IndirectArguments);

            // 初始化Args buffer
            uint[] argsData = new uint[currentArgsOffset];
            int argsIndex = 0;
            foreach (var renderData in prototypeRenderDatas)
            {
                foreach (var lod in renderData.lodLevels)
                {
                    for (int i = 0; i < lod.mesh.subMeshCount; i++)
                    {
                        argsData[argsIndex++] = lod.mesh.GetIndexCount(i);
                        argsData[argsIndex++] = 0; // InstanceCount - 由shader填充
                        argsData[argsIndex++] = lod.mesh.GetIndexStart(i);
                        argsData[argsIndex++] = lod.mesh.GetBaseVertex(i);
                        argsData[argsIndex++] = 0;
                    }
                }
            }

            globalArgsBuffer.SetData(argsData);

            segmentInfoBuffer = new ComputeBuffer(segments.Count, SegmentInfo.Size());
            segmentInfoBuffer.SetData(segments.ToArray());

            Debug.Log($"[TreeInstanceCore] Segmented buffers created:");
            Debug.Log($"  - GlobalInstances: {globalInstances.Count}");
            Debug.Log($"  - GlobalVisibleIndices capacity: {currentIndexOffset}");
            Debug.Log($"  - GlobalArgs size: {currentArgsOffset} uints");
            Debug.Log($"  - Segments: {segments.Count}");
        }
    }

    private void BuildRenderData()
    {
        prototypeRenderDatas.Clear();
        Dictionary<int, PrototypeRenderData> dict = new Dictionary<int, PrototypeRenderData>();

        foreach (var instance in treeData.instances)
        {
            if (!dict.ContainsKey(instance.prototypeIndex))
            {
                GameObject prefab = prototypePrefabs[instance.prototypeIndex];
                if (prefab == null) continue;

                var data = new PrototypeRenderData { prototypeIndex = instance.prototypeIndex, prefab = prefab };
                LODGroup lodGroup = prefab.GetComponent<LODGroup>();

                if (lodGroup != null)
                {
                    LOD[] lods = lodGroup.GetLODs();
                    data.lodDistances = Vector4.zero;

                    float size = lodGroup.size;
                    // 改为：
                    for (int i = 1; i < lods.Length && i < 4; i++)  // 从1开始，跳过LOD 0
                    {
                        float transitionH = Mathf.Max(0.001f, lods[i].screenRelativeTransitionHeight);
                        float dist = (size / transitionH) * 1.5f;
    
                        // 注意：lodDistances存储时要调整索引
                        if (i == 1) data.lodDistances.x = dist;  // LOD 1
                        if (i == 2) data.lodDistances.y = dist;  // LOD 2
                        if (i == 3) data.lodDistances.z = dist;  // LOD 3
    
                        Renderer r = lods[i].renderers.Length > 0 ? lods[i].renderers[0] : null;
                        if (r != null && GetMeshInfo(r, out Mesh m, out Material[] mats))
                        {
                            data.lodLevels.Add(new LODLevelData
                            {
                                lodIndex = i,  // 保持原始索引1,2,3
                                mesh = m, 
                                originalMaterials = mats,
                                instancedMaterials = CreateInstancedMaterials(mats)
                            });
                        }
                    }
                }
                else
                {
                    if (GetMeshInfo(prefab.GetComponentInChildren<Renderer>(), out Mesh m, out Material[] mats))
                    {
                        data.lodLevels.Add(new LODLevelData
                        {
                            lodIndex = 0, mesh = m, originalMaterials = mats,
                            instancedMaterials = CreateInstancedMaterials(mats)
                        });
                        data.lodDistances = new Vector4(99999, 0, 0, 0);
                    }
                }

                if (data.lodLevels.Count > 0) data.boundsSize = data.lodLevels[0].mesh.bounds.size;
                dict[instance.prototypeIndex] = data;
            }

            Vector3 pos = treeData.terrainPosition + Vector3.Scale(instance.position, treeData.terrainSize);
            Quaternion rot = Quaternion.Euler(0, instance.rotation * Mathf.Rad2Deg, 0);
            Vector3 scale = new Vector3(instance.widthScale, instance.heightScale, instance.widthScale);
            dict[instance.prototypeIndex].transforms.Add(Matrix4x4.TRS(pos, rot, scale));
        }

        foreach (var pd in dict.Values)
        {
            prototypeRenderDatas.Add(pd);
            totalInstances += pd.transforms.Count;
        }
    }

    private bool GetMeshInfo(Renderer r, out Mesh mesh, out Material[] mats)
    {
        mesh = null;
        mats = null;
        if (r is MeshRenderer)
        {
            mesh = r.GetComponent<MeshFilter>()?.sharedMesh;
            mats = r.sharedMaterials;
        }
        else if (r is SkinnedMeshRenderer smr)
        {
            mesh = smr.sharedMesh;
            mats = smr.sharedMaterials;
        }

        return mesh != null && mats != null;
    }

    private Material[] CreateInstancedMaterials(Material[] originals)
    {
        if (originals == null || originals.Length == 0)
            return null;

        if (instancedShader == null)
            return originals;

        Material[] instanced = new Material[originals.Length];
        for (int i = 0; i < originals.Length; i++)
        {
            if (originals[i] == null)
                continue;

            instanced[i] = new Material(instancedShader);
            instanced[i].name = originals[i].name + "_Instanced";

            if (originals[i].HasProperty("_MainTex"))
                instanced[i].SetTexture("_BaseMap", originals[i].GetTexture("_MainTex"));
            else if (originals[i].HasProperty("_BaseMap"))
                instanced[i].SetTexture("_BaseMap", originals[i].GetTexture("_BaseMap"));
            else if (originals[i].HasProperty("_Diffuse"))
                instanced[i].SetTexture("_BaseMap", originals[i].GetTexture("_Diffuse"));

            if (originals[i].HasProperty("_Normal"))
                instanced[i].SetTexture("_NormalMap", originals[i].GetTexture("_Normal"));
            else if (originals[i].HasProperty("_BumpMap"))
                instanced[i].SetTexture("_NormalMap", originals[i].GetTexture("_BumpMap"));

            if (originals[i].HasProperty("_NormalPower"))
                instanced[i].SetInt("_NormalScale", originals[i].GetInt("_NormalPower"));

            if (originals[i].HasProperty("_Color"))
                instanced[i].SetColor("_BaseColor", originals[i].GetColor("_Color"));
            else if (originals[i].HasProperty("_BaseColor"))
                instanced[i].SetColor("_BaseColor", originals[i].GetColor("_BaseColor"));
            else if (originals[i].HasProperty("_MainColor"))
                instanced[i].SetColor("_BaseColor", originals[i].GetColor("_MainColor"));
            
            instanced[i].renderQueue = originals[i].renderQueue;
            instanced[i].enableInstancing = true;
        }

        return instanced;
    }

    private void CalculateWorldBounds()
    {
        Vector3 center = treeData.terrainPosition + treeData.terrainSize * 0.5f;
        worldBounds = new Bounds(center, treeData.terrainSize);
    }

    public void Dispose()
    {
        segmentLookupBuffer?.Release();
        debugCountersBuffer?.Release();

        foreach (var rd in prototypeRenderDatas)
        {
            if (rd.lodLevels != null)
            {
                foreach (var lod in rd.lodLevels)
                {
                    if (lod.instancedMaterials != null)
                        foreach (var mat in lod.instancedMaterials)
                            if (mat != null)
                                Object.Destroy(mat);
                }
            }
        }

        prototypeRenderDatas.Clear();

        globalInstanceDataBuffer?.Release();
        globalVisibleIndexBuffer?.Release();
        globalArgsBuffer?.Release();
        segmentInfoBuffer?.Release();

        hzbCuller?.Dispose();
        IsInitialized = false;
    }
}