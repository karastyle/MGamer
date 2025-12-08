// ShaderVariantCollectorWindow.cs
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace EasyShaderCollector
{
    public class ShaderVariantCollectorWindow : EditorWindow
    {
        [MenuItem("Tools/Shader Variant Collector")]
        public static void OpenWindow()
        {
            var window = GetWindow<ShaderVariantCollectorWindow>("Shader Variant Collector");
            window.minSize = new Vector2(500, 300);
        }

        private ShaderVariantCollectorConfig _collectorConfig;
        private Vector2 _scrollPos;
        private string _configPath = "Assets/ShaderVariantCollectorConfig.asset";

        private void OnEnable()
        {
            LoadConfig();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Shader Variant Collector", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawToolbar();
            EditorGUILayout.Space(5);

            if (_collectorConfig == null)
            {
                EditorGUILayout.HelpBox("请先创建或加载配置文件", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("基础配置", EditorStyles.boldLabel);
            _collectorConfig.assetBundleConfig = EditorGUILayout.ObjectField("Asset Bundle Config", _collectorConfig.assetBundleConfig, typeof(AssetBundleConfig), false) as AssetBundleConfig;
            _collectorConfig.savePath = EditorGUILayout.TextField("Save Path", _collectorConfig.savePath);
            _collectorConfig.processCapacity = EditorGUILayout.IntSlider("Process Capacity", _collectorConfig.processCapacity, 100, 5000);
            EditorGUILayout.EndVertical();

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
                bool confirm = EditorUtility.DisplayDialog(
                    "确认",
                    $"开始收集Shader变体?\n\n配置: {_collectorConfig.name}\n输出: {_collectorConfig.savePath}\n容量: {_collectorConfig.processCapacity}",
                    "开始收集",
                    "取消"
                );

                if (confirm)
                {
                    ShaderVariantCollector.Run(_collectorConfig.savePath, _collectorConfig, () =>
                    {
                        EditorUtility.DisplayDialog("完成", $"Shader变体收集完成!\n\n保存位置: {_collectorConfig.savePath}", "OK");
                    });
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New Config", EditorStyles.toolbarButton))
                CreateNewConfig();

            if (GUILayout.Button("Load Config", EditorStyles.toolbarButton))
                LoadConfigDialog();

            if (GUILayout.Button("Save Config", EditorStyles.toolbarButton))
                SaveConfig();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Config", "ShaderVariantCollectorConfig", "asset", "");
            if (!string.IsNullOrEmpty(path))
            {
                _collectorConfig = CreateInstance<ShaderVariantCollectorConfig>();
                AssetDatabase.CreateAsset(_collectorConfig, path);
                AssetDatabase.SaveAssets();
                _configPath = path;
                EditorUtility.SetDirty(_collectorConfig);
            }
        }

        private void LoadConfig()
        {
            _collectorConfig = AssetDatabase.LoadAssetAtPath<ShaderVariantCollectorConfig>(_configPath);
            if (_collectorConfig == null)
            {
                var configs = AssetDatabase.FindAssets("t:ShaderVariantCollectorConfig")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<ShaderVariantCollectorConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToArray();
                if (configs.Length > 0)
                {
                    _collectorConfig = configs[0];
                    _configPath = AssetDatabase.GetAssetPath(_collectorConfig);
                }
            }
        }

        private void LoadConfigDialog()
        {
            string path = EditorUtility.OpenFilePanel("Load Config", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                _collectorConfig = AssetDatabase.LoadAssetAtPath<ShaderVariantCollectorConfig>(path);
                _configPath = path;
            }
        }

        private void SaveConfig()
        {
            if (_collectorConfig != null)
            {
                EditorUtility.SetDirty(_collectorConfig);
                AssetDatabase.SaveAssets();
                Debug.Log("Config saved");
            }
        }
    }
}