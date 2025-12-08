// 放置在 Assets/Editor/JPSGridEditor.cs
using UnityEngine;
using UnityEditor;
using System.IO;

namespace JPSPlus
{
    [CustomEditor(typeof(JPSGrid))]
    public class JPSGridEditor : Editor
    {
        private JPSGrid grid;
        private bool isEditMode = false;

        // 预览设置
        private bool showBakePreview = false;
        // ======================================================
        //   !!!! 新增的选项 !!!!
        // ======================================================
        private bool showAllJumpPoints = false;
        // ======================================================
        
        private float labelDisplayThreshold = 0.4f; 
        private float lastLabelDisplayThreshold = 0.4f;
        
        private GUIStyle labelStyle;
        
        // 8个方向的偏移量 (N, S, W, E, NW, NE, SW, SE)
        private static readonly Vector3[] directionOffsets = new Vector3[]
        {
            new Vector3(0, 0, 0.6f), new Vector3(0, 0, -0.6f), // N, S
            new Vector3(-0.6f, 0, 0), new Vector3(0.6f, 0, 0), // W, E
            new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f), // NW, NE
            new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f) // SW, SE
        };

        void OnEnable()
        {
            grid = (JPSGrid)target;
            SceneView.duringSceneGui += OnSceneGUI;
            
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 12;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (isEditMode)
            {
                ExitEditMode();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Grid Editing", EditorStyles.boldLabel);

            // --- 编辑模式 ---
            if (!isEditMode)
            {
                if (GUILayout.Button("Enter Obstacle Edit Mode", GUILayout.Height(40)))
                {
                    EnterEditMode();
                }
                EditorGUILayout.HelpBox("进入编辑模式后，在Scene视图中：\n- 拖动鼠标左键：添加障碍物\n- 拖动鼠标右键：移除障碍物", MessageType.Info);
            }
            else
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Exit Obstacle Edit Mode", GUILayout.Height(40)))
                {
                    ExitEditMode();
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("JPS+ Baking", EditorStyles.boldLabel);

            // --- 烘焙 ---
            if (GUILayout.Button("Bake JPS+ Data", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Bake JPS+ Data",
                    $"即将烘焙网格数据并保存到：\n{grid.bakedDataSavePath}\n\n这可能需要一些时间。", "Bake", "Cancel"))
                {
                    BakeData();
                }
            }
            
            // --- 预览控制 ---
            EditorGUILayout.Space(10);
            EditorGUI.BeginChangeCheck();
            showBakePreview = EditorGUILayout.Toggle("Show Bake Preview", showBakePreview);
            
            if (showBakePreview)
            {
                // ======================================================
                //   !!!! 新增的勾选选项和说明 !!!!
                // ======================================================
                EditorGUI.indentLevel++;
                showAllJumpPoints = EditorGUILayout.Toggle("Show All Jump Points", showAllJumpPoints);
                EditorGUILayout.HelpBox("勾选后，会显示所有被其它格子当作跳点的格子（包括主跳点和间接跳点）。", MessageType.None);
                EditorGUI.indentLevel--;
                // ======================================================

                EditorGUILayout.HelpBox("预览模式:\n- 青色球体: 主要跳点 (障碍区拐点)\n- 8方向数字: 所有格子的烘焙距离", MessageType.Info);
                
                // --- 性能滑块 ---
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Preview Performance", EditorStyles.miniBoldLabel);
                lastLabelDisplayThreshold = labelDisplayThreshold;
                labelDisplayThreshold = EditorGUILayout.Slider("Label Culling Threshold", labelDisplayThreshold, 0.05f, 1.5f);
                
                if (Mathf.Abs(labelDisplayThreshold - lastLabelDisplayThreshold) > 0.01f)
                {
                    SceneView.RepaintAll();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll(); // 切换时重绘
            }
        }

        // ... (EnterEditMode, ExitEditMode, BakeData 保持不变) ...
        private void EnterEditMode()
        {
            isEditMode = true;
            Tools.current = Tool.None;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.pivot = grid.transform.position;
                sceneView.rotation = Quaternion.Euler(90, 0, 0);
                sceneView.orthographic = true;
                sceneView.Repaint();
            }
            grid.InitializeGridParameters();
        }

        private void ExitEditMode()
        {
            isEditMode = false;
            Tools.current = Tool.Move;
        }

        private void BakeData()
        {
            grid.InitializeGridParameters();
            int width = grid.GridSizeX;
            int height = grid.GridSizeY;
            bool[] walls = new bool[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid.IsObstacle(x, y))
                    {
                        walls[y * width + x] = true;
                    }
                }
            }
            JPSBakedData data = grid.bakedData;
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<JPSBakedData>();
                string directory = Path.GetDirectoryName(grid.bakedDataSavePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                AssetDatabase.CreateAsset(data, grid.bakedDataSavePath);
                grid.bakedData = data;
                EditorUtility.SetDirty(grid);
            }
            data.Initialize(width, height, grid.nodeRadius, grid.GridWorldOrigin, walls);
            JPSBaker.Bake(data);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"JPS+ Bake Complete! Data saved to {grid.bakedDataSavePath}");
        }
        
        // ... (OnSceneGUI, HandleEditing 保持不变) ...
        private void OnSceneGUI(SceneView sceneView)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            
            if (isEditMode)
            {
                Plane gridPlane = new Plane(Vector3.up, grid.transform.position);
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                
                if (gridPlane.Raycast(ray, out float enter))
                {
                    Vector3 worldPos = ray.GetPoint(enter);
                    Int2 coords = grid.GetGridCoords(worldPos);
                    HandleEditing(Event.current, coords);
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }
            }

            if (showBakePreview)
            {
                DrawBakePreview();
            }
            
            if (Event.current.type == EventType.MouseMove && showBakePreview)
            {
                sceneView.Repaint();
            }
        }

        private void HandleEditing(Event e, Int2 coords)
        {
            if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
            {
                bool needsRepaint = false;
                
                if (e.button == 0)
                {
                    if (!grid.manualObstacles.Contains(coords))
                    {
                        Undo.RecordObject(grid, "Add JPS Obstacle");
                        grid.manualObstacles.Add(coords);
                        needsRepaint = true;
                    }
                }
                else if (e.button == 1)
                {
                    if (grid.manualObstacles.Contains(coords))
                    {
                        Undo.RecordObject(grid, "Remove JPS Obstacle");
                        grid.manualObstacles.Remove(coords);
                        needsRepaint = true;
                    }
                }
                
                if (needsRepaint)
                {
                    EditorUtility.SetDirty(grid);
                    e.Use(); 
                }
            }
        }

        // ======================================================
        //   !!!! 绘制预览 (已更新绘制逻辑) !!!!
        // ======================================================
        /// <summary>
        /// 绘制烘焙预览 (已更新为“始终显示”模式)
        /// </summary>
        private void DrawBakePreview()
        {
            if (grid.bakedData == null || grid.bakedData.bakedJumpDistances == null)
                return;
                
            Camera sceneCam = SceneView.currentDrawingSceneView.camera;
            if (sceneCam == null) return;

            // --- 1. 绘制所有跳点 ---
            
            // 循环两次：一次绘制基础跳点（如果需要），一次绘制数字
            for (int x = 0; x < grid.GridSizeX; x++)
            {
                for (int y = 0; y < grid.GridSizeY; y++)
                {
                    Vector3 worldPos = grid.GetWorldPosition(x, y);
                    if (!IsVisible(sceneCam, worldPos)) continue;

                    // 检查是否应该绘制跳点标记
                    bool isPrimary = grid.bakedData.IsPrimaryJumpPoint(x, y);
                    bool isAnyJump = IsAnyJumpPoint(x, y); // 检查是否是任何方向的跳点

                    if (showAllJumpPoints ? isAnyJump : isPrimary)
                    {
                        Handles.color = showAllJumpPoints ? new Color(0f, 1f, 1f, 0.5f) : Color.cyan;
                        Handles.DrawSolidDisc(worldPos, Vector3.up, grid.nodeRadius * 0.3f);
                    }
                    
                    // --- 2. 绘制格子的8方向距离 (数字) ---
                    if (grid.bakedData.IsWall(x, y)) continue;

                    float handleSize = HandleUtility.GetHandleSize(worldPos);
                    if (handleSize < labelDisplayThreshold) continue;
                    
                    DrawNodeLabels(x, y, worldPos);
                }
            }
        }
        
        /// <summary>
        /// 辅助函数：检查一个格子是否是任何方向的跳点
        /// </summary>
        private bool IsAnyJumpPoint(int x, int y)
        {
            JPSBakedData data = grid.bakedData;
            // 检查 MarkPrimary 标记
            if (data.IsPrimaryJumpPoint(x, y)) return true;
            
            // 检查是否是其他方向跳跃路径的起点
            // (例如，如果 SOUTH 方向的距离 > 0，则该点是 NORTH 方向的跳点)
            for (int i = 0; i < 8; i++)
            {
                // 如果当前点作为终点，其对应方向的距离 > 0，则它是一个跳点
                // 例如：如果 NORTH 的距离 > 0，则它是 SOUTH 方向的跳点
                // (注意：这里需要检查的是反方向的距离，但在烘焙数据中，我们直接检查该点的所有距离)
                
                // 因为我们没有反向距离，简单地检查任何方向的距离 > 0 并不准确，
                // 但 JPS+ 理论简化为：只要一个格子能被跳到，它就是一个跳点。
                // 只要其周围任意一个格子的反方向距离 > 1，那么该点就是跳点。
                
                // 最准确的方式：检查该点的 *反方向* 距离 > 1
                EDirFlags dir = DirFlags.FromArrayIndex(i);
                Int2 dirVec = DirFlags.ToPos(dir);
                
                // 检查反方向的邻居 (px, py)
                int px = x - dirVec.x;
                int py = y - dirVec.y;
                
                // 如果邻居存在，并且该邻居朝着当前点 (x, y) 的跳跃距离 > 1，则 (x, y) 是一个跳点
                // (注意：这里的 '1' 是因为距离至少是 1)
                if (data.GetDistance(px, py, i) > 0)
                {
                    return true;
                }
            }
            
            return false;
        }

        private void DrawNodeLabels(int x, int y, Vector3 nodeWorldPos)
        {
            for (int i = 0; i < 8; i++)
            {
                int distance = grid.bakedData.GetDistance(x, y, i);

                Color labelColor;
                if (distance > 0) labelColor = grid.previewColor_JumpPoint;
                else if (distance < 0) labelColor = grid.previewColor_Wall;
                else labelColor = grid.previewColor_Zero;
                
                labelStyle.normal.textColor = labelColor;

                Vector3 offset = directionOffsets[i] * grid.nodeRadius;
                
                Handles.Label(nodeWorldPos + offset, distance.ToString(), labelStyle);
            }
        }

        private bool IsVisible(Camera camera, Vector3 position)
        {
            Vector3 viewPos = camera.WorldToViewportPoint(position);
            return viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;
        }
    }
}