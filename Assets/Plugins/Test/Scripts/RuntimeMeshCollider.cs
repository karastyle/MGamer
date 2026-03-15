using System.Collections;
using System.Linq;
using UnityEngine;
using EasyTools;

public class RuntimeMeshCollider : MonoBehaviour
{
    public string meshAssetName = "YourMeshName";

    [Header("从 FBX 子资产加载")]
    public string fbxAssetName = "";
    public string fbxSubMeshName = "";

    private MeshCollider _meshCollider;
    private bool _isLoading = false;

    void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>();
        if (_meshCollider == null)
            _meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 320));
        GUILayout.BeginVertical("box");

        GUILayout.Label("── 直接加载 Mesh ──");
        GUILayout.Label("Mesh Asset Name:");
        meshAssetName = GUILayout.TextField(meshAssetName);

        GUI.enabled = !_isLoading;
        if (GUILayout.Button(_isLoading ? "加载中..." : "加载并应用 Mesh"))
            LoadAndApplyMesh();
        GUI.enabled = true;

        GUILayout.Space(6);

        GUILayout.Label("── 从 FBX 子资产加载 ──");
        GUILayout.Label("FBX Asset Name:");
        fbxAssetName = GUILayout.TextField(fbxAssetName);
        GUILayout.Label("Sub Mesh Name:");
        fbxSubMeshName = GUILayout.TextField(fbxSubMeshName);

        GUI.enabled = !_isLoading;
        if (GUILayout.Button(_isLoading ? "加载中..." : "从 FBX 加载 Mesh"))
            LoadMeshFromFbx();
        GUI.enabled = true;

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    [ContextMenu("Load And Apply Mesh")]
    public void LoadAndApplyMesh()
    {
        if (_isLoading) return;
        StartCoroutine(LoadMeshCoroutine());
    }

    [ContextMenu("Load Mesh From FBX")]
    public void LoadMeshFromFbx()
    {
        if (_isLoading) return;
        StartCoroutine(LoadMeshFromFbxCoroutine());
    }

    private IEnumerator LoadMeshCoroutine()
    {
        if (string.IsNullOrEmpty(meshAssetName))
        {
            Debug.LogError("[RuntimeMeshCollider] meshAssetName 为空");
            yield break;
        }

        _isLoading = true;

        AssetHandle handle = EasyAsset.Instance.LoadAssetAsync(meshAssetName);
        yield return handle.WaitForCompletion();

        var mesh = handle.AssetObject<Mesh>();
        if (mesh == null)
        {
            Debug.LogError($"[RuntimeMeshCollider] 加载失败或类型不是 Mesh：{meshAssetName}");
            _isLoading = false;
            yield break;
        }

        ApplyMesh(mesh);
        _isLoading = false;
    }

    private IEnumerator LoadMeshFromFbxCoroutine()
    {
        if (string.IsNullOrEmpty(fbxAssetName))
        {
            Debug.LogError("[RuntimeMeshCollider] fbxAssetName 为空");
            yield break;
        }
        if (string.IsNullOrEmpty(fbxSubMeshName))
        {
            Debug.LogError("[RuntimeMeshCollider] fbxSubMeshName 为空");
            yield break;
        }

        _isLoading = true;

        SubAssetsHandle handle = EasyAsset.Instance.LoadSubAssetsAsync(fbxAssetName);
        yield return handle.WaitForCompletion();

        Mesh mesh = handle.GetSubAssets<Mesh>()?.FirstOrDefault(m => m.name == fbxSubMeshName);
        if (mesh == null)
        {
            Debug.LogError($"[RuntimeMeshCollider] 在 FBX [{fbxAssetName}] 中未找到名为 [{fbxSubMeshName}] 的 Mesh");
            _isLoading = false;
            yield break;
        }

        ApplyMesh(mesh);
        _isLoading = false;
    }

    private void ApplyMesh(Mesh mesh)
    {
        _meshCollider.sharedMesh = mesh;
        Debug.Log($"[RuntimeMeshCollider] 已成功将 Mesh [{mesh.name}] 赋值到 MeshCollider");

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
            Debug.Log("[RuntimeMeshCollider] 已取消 Freeze Position Y");
        }
        else
        {
            Debug.LogWarning("[RuntimeMeshCollider] 未找到 Rigidbody，无法修改 constraints");
        }
    }
}