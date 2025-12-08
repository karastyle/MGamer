using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridSplitManager : MonoBehaviour
{
    [Header("区域设置")] public Vector2 totalSize = new Vector2(1000f, 1000f);
    public Vector2 cellSize = new Vector2(10f, 10f);

    [Header("Gizmo设置")] public Color gridColor = Color.green;
    public Color lockedOverlayColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("节点设置")] public string nodePrefix = "Node";

    [Header("Prefab设置")] public string prefabSavePath = "Assets/Prefabs/GridNodes";

    [Header("节点锁定")] public List<string> lockedNodeNames = new List<string>();

    [Header("对象分配")] public List<Transform> sourceTransforms = new List<Transform>();

    [Header("对象提取")] public Transform extractRoot;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Gizmos.color = gridColor;

        int gridX = Mathf.CeilToInt(totalSize.x / cellSize.x);
        int gridZ = Mathf.CeilToInt(totalSize.y / cellSize.y);

        // 绘制外边框
        Gizmos.DrawLine(origin, origin + new Vector3(totalSize.x, 0, 0));
        Gizmos.DrawLine(origin + new Vector3(totalSize.x, 0, 0), origin + new Vector3(totalSize.x, 0, totalSize.y));
        Gizmos.DrawLine(origin + new Vector3(totalSize.x, 0, totalSize.y), origin + new Vector3(0, 0, totalSize.y));
        Gizmos.DrawLine(origin + new Vector3(0, 0, totalSize.y), origin);

        // 绘制垂直线
        for (int x = 1; x < gridX; x++)
        {
            float xPos = x * cellSize.x;
            Gizmos.DrawLine(origin + new Vector3(xPos, 0, 0), origin + new Vector3(xPos, 0, totalSize.y));
        }

        // 绘制水平线
        for (int z = 1; z < gridZ; z++)
        {
            float zPos = z * cellSize.y;
            Gizmos.DrawLine(origin + new Vector3(0, 0, zPos), origin + new Vector3(totalSize.x, 0, zPos));
        }

        // 绘制锁定节点的半透明蒙版
        foreach (Transform child in transform)
        {
            if (IsNodeLocked(child.name))
            {
                Vector3 nodePos = child.position;
                Vector3 center = nodePos + new Vector3(cellSize.x * 0.5f, 0, cellSize.y * 0.5f);

                // 绘制半透明方块
                Gizmos.color = lockedOverlayColor;
                Vector3 size = new Vector3(cellSize.x, 0.1f, cellSize.y);
                Gizmos.DrawCube(center, size);

                // 绘制边框
                Gizmos.color = Color.red;
                Vector3[] corners = new Vector3[]
                {
                    nodePos,
                    nodePos + new Vector3(cellSize.x, 0, 0),
                    nodePos + new Vector3(cellSize.x, 0, cellSize.y),
                    nodePos + new Vector3(0, 0, cellSize.y)
                };

                for (int i = 0; i < 4; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
                }
            }
        }
    }


    public void PerformSplit()
    {
        int gridX = Mathf.CeilToInt(totalSize.x / cellSize.x);
        int gridZ = Mathf.CeilToInt(totalSize.y / cellSize.y);

        int created = 0;

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                string nodeName = $"{nodePrefix}_{x}_{z}";
                Transform existing = transform.Find(nodeName);

                if (existing != null) continue;

                GameObject node = new GameObject(nodeName);
                node.transform.SetParent(transform);
                node.transform.position = transform.position + new Vector3(x * cellSize.x, 0, z * cellSize.y);

                GridCell cell = node.AddComponent<GridCell>();
                cell.cellSize = cellSize;
                cell.cellColor = gridColor;

                created++;
            }
        }

        Debug.Log($"创建了 {created} 个节点,跳过 {gridX * gridZ - created} 个已存在的节点");
    }

    public void DistributeObjects()
    {
        if (sourceTransforms == null || sourceTransforms.Count == 0)
        {
            Debug.LogWarning("源Transform列表为空");
            return;
        }

        int totalDistributed = 0;
        int totalSkipped = 0;

        foreach (Transform sourceTransform in sourceTransforms)
        {
            if (sourceTransform == null) continue;

            List<Transform> children = new List<Transform>();
            foreach (Transform child in sourceTransform)
            {
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                Transform targetNode = GetNodeAtPosition(child.position);

                if (targetNode != null)
                {
                    child.SetParent(targetNode);
                    totalDistributed++;
                }
                else
                {
                    totalSkipped++;
                }
            }
        }

        Debug.Log($"对象分配完成：已分配 {totalDistributed} 个，跳过 {totalSkipped} 个（超出范围）");
    }

    public void ExtractAllObjects()
    {
        if (extractRoot == null)
        {
            Debug.LogWarning("Extract Root未设置");
            return;
        }

        int totalExtracted = 0;

        foreach (Transform node in transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in node)
            {
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                child.SetParent(extractRoot);
                totalExtracted++;
            }
        }

        Debug.Log($"对象提取完成：已提取 {totalExtracted} 个对象到 {extractRoot.name}");
    }

    public void SaveNodesToPrefabs()
    {
        if (!AssetDatabase.IsValidFolder(prefabSavePath))
        {
            string[] folders = prefabSavePath.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }

                currentPath = newPath;
            }
        }

        int savedCount = 0;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            string prefabPath = $"{prefabSavePath}/{child.name}.prefab";

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(child.gameObject, prefabPath);

            if (prefabAsset != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                instance.name = child.name;
                instance.transform.SetParent(transform);
                instance.transform.position = child.position;
                instance.transform.rotation = child.rotation;
                instance.transform.localScale = child.localScale;

                DestroyImmediate(child.gameObject);

                savedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"已保存 {savedCount} 个节点为Prefab，并在场景中关联");
    }

    public bool IsNodeLocked(string nodeName)
    {
        return lockedNodeNames.Contains(nodeName);
    }

    public void SetNodeLocked(string nodeName, bool locked)
    {
        if (locked && !lockedNodeNames.Contains(nodeName))
        {
            lockedNodeNames.Add(nodeName);
            SetNodeSelectableRecursive(transform.Find(nodeName), false);
        }
        else if (!locked && lockedNodeNames.Contains(nodeName))
        {
            lockedNodeNames.Remove(nodeName);
            SetNodeSelectableRecursive(transform.Find(nodeName), true);
        }
    }

    public void LockAll()
    {
        foreach (Transform child in transform)
        {
            SetNodeLocked(child.name, true);
        }
    }

    public void UnlockAll()
    {
        foreach (Transform child in transform)
        {
            SetNodeLocked(child.name, false);
        }
    }

    private void SetNodeSelectableRecursive(Transform node, bool selectable)
    {
        if (node == null) return;

        node.gameObject.hideFlags = selectable ? HideFlags.None : HideFlags.NotEditable;

        foreach (Transform child in node)
        {
            SetNodeSelectableRecursive(child, selectable);
        }
    }

    public void ApplyPrefabOverrides()
    {
        int appliedCount = 0;
        int skippedCount = 0;

        foreach (Transform child in transform)
        {
            if (IsNodeLocked(child.name))
            {
                skippedCount++;
                continue;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(child.gameObject))
            {
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(child.gameObject);
                PrefabUtility.ApplyPrefabInstance(prefabRoot, InteractionMode.UserAction);
                appliedCount++;
                Debug.Log($"已应用 {child.name} 的Prefab Override");
            }
        }

        Debug.Log($"Prefab Override完成：应用 {appliedCount} 个，跳过 {skippedCount} 个（已锁定）");

        AssetDatabase.SaveAssets();
    }

    public Transform GetNodeAtPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;

        int x = Mathf.FloorToInt(localPos.x / cellSize.x);
        int z = Mathf.FloorToInt(localPos.z / cellSize.y);

        if (x < 0 || z < 0 || x >= Mathf.CeilToInt(totalSize.x / cellSize.x) ||
            z >= Mathf.CeilToInt(totalSize.y / cellSize.y))
        {
            return null;
        }

        string nodeName = $"{nodePrefix}_{x}_{z}";
        return transform.Find(nodeName);
    }
#endif
}