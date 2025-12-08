using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridSplitManager))]
public class GridSplitManagerEditor : Editor
{
    private Vector2 scrollPosition;
    private bool isLockingMode = false;
    private bool lockModeToggle = true; // true=锁定, false=解锁

    private void OnEnable()
    {
        isLockingMode = false;
    }

    private void OnDisable()
    {
        isLockingMode = false;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        GridSplitManager manager = (GridSplitManager)target;
        
        int gridX = Mathf.CeilToInt(manager.totalSize.x / manager.cellSize.x);
        int gridZ = Mathf.CeilToInt(manager.totalSize.y / manager.cellSize.y);
        
        EditorGUILayout.HelpBox(
            $"将拆分为 {gridX} × {gridZ} = {gridX * gridZ} 个格子\n" +
            $"当前子节点数量: {manager.transform.childCount}",
            MessageType.Info
        );
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("执行拆分", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("确认拆分", 
                $"将创建 {gridX * gridZ} 个节点（已存在的会跳过）\n是否继续？", 
                "确定", "取消"))
            {
                Undo.RegisterCompleteObjectUndo(manager.gameObject, "Grid Split");
                manager.PerformSplit();
                EditorUtility.SetDirty(manager.gameObject);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // 对象分配按钮
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("分配对象到节点", GUILayout.Height(40)))
        {
            if (manager.sourceTransforms == null || manager.sourceTransforms.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先在Source Transforms列表中添加Transform对象", "确定");
            }
            else if (EditorUtility.DisplayDialog("确认分配", 
                $"将把列表中所有Transform的子对象分配到对应节点\n是否继续？", 
                "确定", "取消"))
            {
                Undo.RegisterCompleteObjectUndo(manager.transform, "Distribute Objects");
                manager.DistributeObjects();
                EditorUtility.SetDirty(manager.gameObject);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        // 对象提取按钮
        GUI.backgroundColor = new Color(1f, 0.7f, 0.5f);
        if (GUILayout.Button("提取所有对象到Extract Root", GUILayout.Height(40)))
        {
            if (manager.extractRoot == null)
            {
                EditorUtility.DisplayDialog("提示", "请先设置Extract Root", "确定");
            }
            else if (EditorUtility.DisplayDialog("确认提取", 
                $"将把所有节点中的对象移动到 {manager.extractRoot.name}\n是否继续？", 
                "确定", "取消"))
            {
                Undo.RegisterCompleteObjectUndo(manager.transform, "Extract Objects");
                if (manager.extractRoot != null)
                {
                    Undo.RegisterCompleteObjectUndo(manager.extractRoot, "Extract Objects");
                }
                manager.ExtractAllObjects();
                EditorUtility.SetDirty(manager.gameObject);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        if (manager.transform.childCount > 0)
        {
            // 可视化锁定模式
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("可视化锁定工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            lockModeToggle = EditorGUILayout.Toggle("锁定模式", lockModeToggle, GUILayout.Width(150));
            EditorGUILayout.LabelField(lockModeToggle ? "点击=锁定" : "点击=解锁", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            if (!isLockingMode)
            {
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("开始可视化锁定", GUILayout.Height(35)))
                {
                    isLockingMode = true;
                    SceneView.duringSceneGui += OnSceneGUI;
                    
                    // 切换到俯视图
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    if (sceneView != null)
                    {
                        sceneView.orthographic = true;
                        sceneView.rotation = Quaternion.Euler(90, 0, 0);
                        sceneView.Frame(new Bounds(manager.transform.position + new Vector3(manager.totalSize.x * 0.5f, 0, manager.totalSize.y * 0.5f), 
                                                   new Vector3(manager.totalSize.x, 0, manager.totalSize.y)), false);
                        sceneView.Repaint();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("退出可视化锁定", GUILayout.Height(35)))
                {
                    isLockingMode = false;
                    SceneView.duringSceneGui -= OnSceneGUI;
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.HelpBox(
                    "🖱️ 在Scene视图中点击格子进行" + (lockModeToggle ? "锁定" : "解锁") + "\n" +
                    "💡 已锁定的格子显示为红色边框+灰色蒙版",
                    MessageType.Info
                );
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 快捷操作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全部锁定", GUILayout.Height(30)))
            {
                Undo.RegisterCompleteObjectUndo(manager, "Lock All");
                manager.LockAll();
                EditorUtility.SetDirty(manager);
            }
            if (GUILayout.Button("全部解锁", GUILayout.Height(30)))
            {
                Undo.RegisterCompleteObjectUndo(manager, "Unlock All");
                manager.UnlockAll();
                EditorUtility.SetDirty(manager);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 节点锁定列表
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("子节点列表", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));
            
            foreach (Transform child in manager.transform)
            {
                EditorGUILayout.BeginHorizontal();
                
                bool isLocked = manager.IsNodeLocked(child.name);
                bool newLocked = EditorGUILayout.ToggleLeft("🔒", isLocked, GUILayout.Width(30));
                
                if (newLocked != isLocked)
                {
                    Undo.RegisterCompleteObjectUndo(manager, "Toggle Node Lock");
                    manager.SetNodeLocked(child.name, newLocked);
                    EditorUtility.SetDirty(manager);
                }
                
                GUI.enabled = !isLocked;
                EditorGUILayout.LabelField(child.name);
                
                bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(child.gameObject);
                if (isPrefabInstance)
                {
                    EditorGUILayout.LabelField("[Prefab]", GUILayout.Width(60));
                }
                
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Apply Prefab Override按钮
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Apply Prefab Override（未锁定）", GUILayout.Height(40)))
            {
                int unlockedCount = 0;
                foreach (Transform child in manager.transform)
                {
                    if (!manager.IsNodeLocked(child.name))
                        unlockedCount++;
                }
                
                if (EditorUtility.DisplayDialog("确认应用", 
                    $"将对 {unlockedCount} 个未锁定的节点应用Prefab Override\n是否继续？", 
                    "确定", "取消"))
                {
                    manager.ApplyPrefabOverrides();
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space(5);
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("保存为Prefab", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("确认保存", 
                    $"将保存 {manager.transform.childCount} 个节点为Prefab\n并在场景中关联Prefab实例\n是否继续？", 
                    "确定", "取消"))
                {
                    manager.SaveNodesToPrefabs();
                    EditorUtility.SetDirty(manager.gameObject);
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isLockingMode) return;
        
        GridSplitManager manager = (GridSplitManager)target;
        
        Event e = Event.current;
        
        // 绘制提示信息
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.BeginVertical("box");
        GUILayout.Label("可视化锁定模式", EditorStyles.boldLabel);
        GUILayout.Label($"当前模式: {(lockModeToggle ? "锁定" : "解锁")}");
        GUILayout.Label("点击格子进行操作");
        GUILayout.Label("按ESC退出");
        GUILayout.EndVertical();
        GUILayout.EndArea();
        Handles.EndGUI();
        
        // 处理鼠标点击
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, manager.transform.position);
            
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Transform node = manager.GetNodeAtPosition(hitPoint);
                
                if (node != null)
                {
                    Undo.RegisterCompleteObjectUndo(manager, "Toggle Node Lock");
                    manager.SetNodeLocked(node.name, lockModeToggle);
                    EditorUtility.SetDirty(manager);
                    
                    e.Use();
                    sceneView.Repaint();
                }
            }
        }
        
        // 按ESC退出
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            isLockingMode = false;
            SceneView.duringSceneGui -= OnSceneGUI;
            e.Use();
        }
        
        // 阻止选中物体
        if (isLockingMode)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}