using System.Collections;
using UnityEngine;
using EasyTools;

public class RuntimeMeshCollider : MonoBehaviour
{
    public string meshAssetName = "YourMeshName";

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
        GUILayout.BeginArea(new Rect(20, 20, 300, 120));
        GUILayout.BeginVertical("box");

        GUILayout.Label("Mesh Asset Name:");
        meshAssetName = GUILayout.TextField(meshAssetName);

        GUI.enabled = !_isLoading;
        if (GUILayout.Button(_isLoading ? "加载中..." : "加载并应用 Mesh"))
            LoadAndApplyMesh();
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

        _meshCollider.sharedMesh = mesh;
        Debug.Log($"[RuntimeMeshCollider] 已成功将 Mesh [{meshAssetName}] 赋值到 MeshCollider");

        // 取消 Freeze Position Y 约束
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
        
        _isLoading = false;
    }
}