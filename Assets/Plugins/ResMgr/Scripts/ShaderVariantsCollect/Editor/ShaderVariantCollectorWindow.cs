// ShaderVariantCollectorWindow.cs
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace EasyShaderCollector
{
    public class ShaderVariantCollectorWindow : EditorWindow
    {
        private const string PrefKeyLastConfigPath = "ShaderVariantCollector_LastConfigPath";

        [MenuItem("Tools/Shader Variant Collector")]
        public static void OpenWindow()
        {
            var window = GetWindow<ShaderVariantCollectorWindow>("Shader Variant Collector");
            window.minSize = new Vector2(500, 300);
        }

        private ShaderVariantCollectorConfig _collectorConfig;
        private Vector2 _scrollPos;

        private ShaderVariantCollectorConfig[] _allConfigs;
        private string[] _configNames;
        private int _selectedConfigIndex;

        private void OnEnable()
        {
            LoadLastConfig();
        }

        private void RefreshConfigList()
        {
            _allConfigs = AssetDatabase.FindAssets("t:ShaderVariantCollectorConfig")
                .Select(g => AssetDatabase.LoadAssetAtPath<ShaderVariantCollectorConfig>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null)
                .ToArray();
            _configNames = _allConfigs.Select(c => AssetDatabase.GetAssetPath(c)).ToArray();

            _selectedConfigIndex = -1;
            if (_collectorConfig != null)
                for (int i = 0; i < _allConfigs.Length; i++)
                    if (_allConfigs[i] == _collectorConfig) { _selectedConfigIndex = i; break; }
        }

        private void LoadLastConfig()
        {
            string lastPath = EditorPrefs.GetString(PrefKeyLastConfigPath, "");
            if (!string.IsNullOrEmpty(lastPath))
                _collectorConfig = AssetDatabase.LoadAssetAtPath<ShaderVariantCollectorConfig>(lastPath);

            RefreshConfigList();

            if (_collectorConfig == null && _allConfigs.Length > 0)
            {
                _collectorConfig = _allConfigs[0];
                _selectedConfigIndex = 0;
                PersistLastConfigPath();
            }
        }

        private void PersistLastConfigPath()
        {
            if (_collectorConfig == null) return;
            EditorPrefs.SetString(PrefKeyLastConfigPath, AssetDatabase.GetAssetPath(_collectorConfig));
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Shader Variant Collector", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawConfigBar();
            EditorGUILayout.Space(5);

            if (_collectorConfig == null)
            {
                EditorGUILayout.HelpBox("请先创建配置文件", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("基础配置", EditorStyles.boldLabel);
            _collectorConfig.assetBundleConfig = EditorGUILayout.ObjectField(
                "Asset Bundle Config", _collectorConfig.assetBundleConfig, typeof(AssetBundleConfig), false) as AssetBundleConfig;
            DrawSavePathFields();
            _collectorConfig.processCapacity = EditorGUILayout.IntSlider("Process Capacity", _collectorConfig.processCapacity, 100, 5000);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_collectorConfig);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
            int shaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
            int variantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
            EditorGUILayout.LabelField($"Shader Count: {shaderCount}");
            EditorGUILayout.LabelField($"Variant Count: {variantCount}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            GUI.enabled = _collectorConfig.assetBundleConfig != null;
            if (GUILayout.Button("开始收集 Shader Variants", GUILayout.Height(35)))
            {
                string savePath = _collectorConfig.GetSavePath();
                bool confirm = EditorUtility.DisplayDialog(
                    "确认",
                    $"开始收集Shader变体?\n\n配置: {_collectorConfig.name}\n输出: {savePath}\n容量: {_collectorConfig.processCapacity}",
                    "开始收集",
                    "取消"
                );

                if (confirm)
                {
                    ShaderVariantCollector.Run(savePath, _collectorConfig, () =>
                    {
                        EditorUtility.DisplayDialog("完成", $"Shader变体收集完成!\n\n保存位置: {savePath}", "OK");
                    });
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigBar()
        {
            // 第一行：New Config 按钮
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("New Config", EditorStyles.toolbarButton, GUILayout.Width(80)))
                CreateNewConfig();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // 第二行：配置下拉，独占一行
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Config", EditorStyles.toolbarButton, GUILayout.Width(50));
            if (_allConfigs != null && _allConfigs.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                _selectedConfigIndex = EditorGUILayout.Popup(_selectedConfigIndex, _configNames,
                    EditorStyles.toolbarPopup);
                if (EditorGUI.EndChangeCheck() && _selectedConfigIndex >= 0 && _selectedConfigIndex < _allConfigs.Length)
                {
                    _collectorConfig = _allConfigs[_selectedConfigIndex];
                    PersistLastConfigPath();
                }
            }
            else
            {
                GUILayout.Label("无配置文件", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSavePathFields()
        {
            // Folder field with browse button and drag-and-drop
            EditorGUILayout.BeginHorizontal();
            _collectorConfig.saveFolder = EditorGUILayout.TextField("Save Folder", _collectorConfig.saveFolder);
            Rect folderFieldRect = GUILayoutUtility.GetLastRect();
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择保存目录", _collectorConfig.saveFolder, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                        selected = "Assets" + selected.Substring(Application.dataPath.Length);
                    _collectorConfig.saveFolder = selected;
                    EditorUtility.SetDirty(_collectorConfig);
                    AssetDatabase.SaveAssets();
                }
            }
            EditorGUILayout.EndHorizontal();

            HandleFolderDrop(folderFieldRect);

            // File name field
            _collectorConfig.saveFileName = EditorGUILayout.TextField("Save File Name", _collectorConfig.saveFileName);

            // Read-only full path preview
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Full Path", _collectorConfig.GetSavePath());
            EditorGUI.EndDisabledGroup();
        }

        private void HandleFolderDrop(Rect rect)
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
            if (!rect.Contains(evt.mousePosition)) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        _collectorConfig.saveFolder = path;
                        EditorUtility.SetDirty(_collectorConfig);
                        AssetDatabase.SaveAssets();
                        break;
                    }
                }
            }
            evt.Use();
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Config", "ShaderVariantCollectorConfig", "asset", "");
            if (!string.IsNullOrEmpty(path))
            {
                var config = CreateInstance<ShaderVariantCollectorConfig>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                _collectorConfig = config;
                RefreshConfigList();
                PersistLastConfigPath();
            }
        }
    }
}
