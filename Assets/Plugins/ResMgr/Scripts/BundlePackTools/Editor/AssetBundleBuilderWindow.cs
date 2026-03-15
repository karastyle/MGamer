// AssetBundleBuilderWindow.cs

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.IO.Compression;
using EasyTools;

public class AssetBundleBuilderWindow : EditorWindow
{
    // ── 核心数据 ──────────────────────────────────────────────
    private AssetBundleConfig config;
    private Vector2 groupScrollPos;
    private Vector2 collectorScrollPos;
    private Vector2 sceneScrollPos;
    private int selectedGroupIndex = -1;
    private string cdnPath = "";

    // ── Config 下拉 ───────────────────────────────────────────
    private AssetBundleConfig[] allConfigs = new AssetBundleConfig[0];
    private string[] allConfigNames = new string[0];
    private int selectedConfigIndex = -1;

    // ── 平台 & 安装 ───────────────────────────────────────────
    private BuildTarget selectedTarget = BuildTarget.StandaloneWindows64;
    private bool autoInstallAfterBuild = false;
    private List<string> connectedDevices = new List<string>();
    private string[] deviceDisplayNames = new string[0];
    private int selectedDeviceIndex = -1;

    // ── Tab ───────────────────────────────────────────────────
    private int _selectedTab = 0;
    private static readonly string[] _tabNames = { "AB规则配置", "打 AB", "打安装包" };

    // ── Collector 展开 & 资源缓存 ────────────────────────────
    // key = groupIndex * 10000 + collectorIndex
    private Dictionary<int, bool> _collectorFoldouts = new Dictionary<int, bool>();
    private Dictionary<int, List<string>> _collectorAssetCache = new Dictionary<int, List<string>>();

    // ── 自动保存 ──────────────────────────────────────────────
    private bool _pendingSave = false;
    private double _lastSaveTime = 0;

    // ── EditorPrefs Keys ──────────────────────────────────────
    private const string kCDNPath     = "AssetBundleBuilder_CDNPath";
    private const string kLastConfig  = "AssetBundleBuilder_LastConfigPath";
    private const string kPlatform    = "AssetBundleBuilder_Platform";
    private const string kAutoInstall = "AssetBundleBuilder_AutoInstall";
    private const string kSelectedTab = "AssetBundleBuilder_Tab";

    // ─────────────────────────────────────────────────────────
    [MenuItem("Tools/AssetBundle Builder")]
    static void ShowWindow()
    {
        var window = GetWindow<AssetBundleBuilderWindow>("AssetBundle Builder");
        window.minSize = new Vector2(900, 600);
    }

    void OnEnable()
    {
        cdnPath               = EditorPrefs.GetString(kCDNPath, "");
        selectedTarget        = (BuildTarget)EditorPrefs.GetInt(kPlatform, (int)BuildTarget.StandaloneWindows64);
        autoInstallAfterBuild = EditorPrefs.GetBool(kAutoInstall, false);
        _selectedTab          = EditorPrefs.GetInt(kSelectedTab, 0);

        string lastPath = EditorPrefs.GetString(kLastConfig, "");
        if (!string.IsNullOrEmpty(lastPath))
            config = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(lastPath);

        RefreshConfigList();
        if (config == null) LoadConfig();
    }

    void OnDisable()
    {
        EditorPrefs.SetString(kCDNPath, cdnPath);
        EditorPrefs.SetInt(kPlatform, (int)selectedTarget);
        EditorPrefs.SetBool(kAutoInstall, autoInstallAfterBuild);
        EditorPrefs.SetInt(kSelectedTab, _selectedTab);
        if (_pendingSave) DoSaveConfig();
    }

    // ── Config 下拉刷新 ───────────────────────────────────────
    void RefreshConfigList()
    {
        allConfigs = AssetDatabase.FindAssets("t:AssetBundleConfig")
            .Select(guid => AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(c => c != null).ToArray();
        allConfigNames = allConfigs.Select(c => AssetDatabase.GetAssetPath(c)).ToArray();

        selectedConfigIndex = -1;
        if (config != null)
            for (int i = 0; i < allConfigs.Length; i++)
                if (allConfigs[i] == config) { selectedConfigIndex = i; break; }
    }

    // ─────────────────────────────────────────────────────────
    void OnGUI()
    {
        DrawConfigSelector();
        DrawToolbar();

        EditorGUI.BeginChangeCheck();
        DrawMainLayout();
        if (EditorGUI.EndChangeCheck() && config != null)
        {
            EditorUtility.SetDirty(config);
            _pendingSave = true;
        }

        if (_pendingSave && EditorApplication.timeSinceStartup - _lastSaveTime >= 1.0)
            DoSaveConfig();
    }

    // ── Config 选择栏 ─────────────────────────────────────────
    void DrawConfigSelector()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Config", GUILayout.Width(45));

        if (allConfigs.Length == 0)
        {
            EditorGUILayout.LabelField("No AssetBundleConfig found", EditorStyles.miniLabel);
        }
        else
        {
            int newIndex = EditorGUILayout.Popup(selectedConfigIndex, allConfigNames);
            if (newIndex != selectedConfigIndex && newIndex >= 0 && newIndex < allConfigs.Length)
            {
                selectedConfigIndex = newIndex;
                config = allConfigs[selectedConfigIndex];
                EditorPrefs.SetString(kLastConfig, allConfigNames[selectedConfigIndex]);
                selectedGroupIndex = -1;
                _collectorFoldouts.Clear();
                _collectorAssetCache.Clear();
            }
        }

        if (GUILayout.Button("↺", GUILayout.Width(24))) RefreshConfigList();
        EditorGUILayout.EndHorizontal();
    }

    // ── Toolbar ───────────────────────────────────────────────
    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("New Config",  EditorStyles.toolbarButton)) CreateNewConfig();
        if (GUILayout.Button("Load Config", EditorStyles.toolbarButton)) LoadConfigDialog();
        if (GUILayout.Button("Export",      EditorStyles.toolbarButton)) ExportConfig();
        if (GUILayout.Button("Import",      EditorStyles.toolbarButton)) ImportConfig();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // ── 主布局 ────────────────────────────────────────────────
    void DrawMainLayout()
    {
        if (config == null)
        {
            EditorGUILayout.HelpBox("Please create or load a config", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(4);
        int prevTab = _selectedTab;
        _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames, GUILayout.Height(28));
        if (_selectedTab != prevTab)
        {
            // 切换 tab 时清资源缓存，避免显示过期数据
            _collectorAssetCache.Clear();
            EditorPrefs.SetInt(kSelectedTab, _selectedTab);
        }
        EditorGUILayout.Space(3);

        switch (_selectedTab)
        {
            case 0: DrawRulesTab();  break;
            case 1: DrawAbTab();     break;
            case 2: DrawPlayerTab(); break;
        }
    }

    // ══════════════════════════════════════════════════════════
    // Tab 0：AB规则配置
    // ══════════════════════════════════════════════════════════
    void DrawRulesTab()
    {
        EditorGUILayout.BeginHorizontal();
        DrawGroupsPanel();
        DrawCollectorsPanel();
        EditorGUILayout.EndHorizontal();
    }

    void DrawGroupsPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.28f));
        EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
        groupScrollPos = EditorGUILayout.BeginScrollView(groupScrollPos, "box", GUILayout.ExpandHeight(true));
        for (int i = 0; i < config.collectorGroups.Count; i++)
            DrawGroupItem(config.collectorGroups[i], i);
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Add Group", GUILayout.Height(25)))
        {
            config.collectorGroups.Add(new CollectorGroup { groupName = "Group " + config.collectorGroups.Count });
            EditorUtility.SetDirty(config);
        }
        EditorGUILayout.EndVertical();
    }

    void DrawGroupItem(CollectorGroup group, int index)
    {
        bool isSelected = selectedGroupIndex == index;
        Color orig = GUI.backgroundColor;
        if (isSelected) GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            config.collectorGroups.RemoveAt(index);
            if (selectedGroupIndex == index) selectedGroupIndex = -1;
            EditorUtility.SetDirty(config);
            _collectorFoldouts.Clear();
            _collectorAssetCache.Clear();
            GUI.backgroundColor = orig;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        if (GUILayout.Button(group.groupName, EditorStyles.label))
        {
            selectedGroupIndex = index;
            _collectorAssetCache.Clear(); // 切换 group 清缓存
        }
        group.active = EditorGUILayout.Toggle(group.active, GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();
        if (isSelected)
            group.groupName = EditorGUILayout.TextField("Name", group.groupName);
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = orig;
    }

    void DrawCollectorsPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.72f - 20));
        EditorGUILayout.LabelField("Collectors", EditorStyles.boldLabel);

        if (selectedGroupIndex < 0 || selectedGroupIndex >= config.collectorGroups.Count)
        {
            EditorGUILayout.HelpBox("Select a group to view collectors", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        var group = config.collectorGroups[selectedGroupIndex];
        collectorScrollPos = EditorGUILayout.BeginScrollView(collectorScrollPos, "box", GUILayout.ExpandHeight(true));
        for (int i = 0; i < group.collectors.Count; i++)
            DrawCollectorItem(group.collectors[i], i, group);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Collector", GUILayout.Height(25)))
        {
            group.collectors.Add(new Collector());
            EditorUtility.SetDirty(config);
        }
        EditorGUILayout.EndVertical();
    }

    void DrawCollectorItem(Collector collector, int index, CollectorGroup group)
    {
        int cacheKey = selectedGroupIndex * 10000 + index;

        EditorGUILayout.BeginVertical("box");

        // ── 第一行：删除 + 路径 ────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            group.collectors.RemoveAt(index);
            EditorUtility.SetDirty(config);
            _collectorFoldouts.Remove(cacheKey);
            _collectorAssetCache.Remove(cacheKey);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.LabelField("Collector", GUILayout.Width(60));
        var prevPath = collector.collectorPath;
        collector.collectorPath = EditorGUILayout.ObjectField(collector.collectorPath, typeof(UnityEngine.Object), false);
        if (collector.collectorPath != prevPath)
            _collectorAssetCache.Remove(cacheKey); // 路径变了，清缓存
        EditorGUILayout.EndHorizontal();

        // ── 第二行：规则 ──────────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(25);
        EditorGUILayout.LabelField("Pack Rule", GUILayout.Width(70));
        var prevPack = collector.packRule;
        collector.packRule = (PackRule)EditorGUILayout.EnumPopup(collector.packRule, GUILayout.Width(150));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Collect Type", GUILayout.Width(80));
        var prevCollect = collector.collectType;
        collector.collectType = (CollectType)EditorGUILayout.EnumPopup(collector.collectType, GUILayout.Width(150));
        if (collector.packRule != prevPack || collector.collectType != prevCollect)
            _collectorAssetCache.Remove(cacheKey); // 规则变了，清缓存
        EditorGUILayout.EndHorizontal();

        // ── 第三行：Tag ───────────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(25);
        EditorGUILayout.LabelField("Asset Tag", GUILayout.Width(70));
        collector.assetTag = (AssetTag)EditorGUILayout.EnumPopup(collector.assetTag, GUILayout.Width(480));
        EditorGUILayout.EndHorizontal();

        // ── 资源展开列表 ──────────────────────────────────────
        if (collector.collectorPath != null)
        {
            if (!_collectorFoldouts.TryGetValue(cacheKey, out bool foldout))
                foldout = false;

            // 取或扫描资源列表
            if (!_collectorAssetCache.TryGetValue(cacheKey, out var assets))
            {
                assets = ScanCollectorAssets(collector);
                _collectorAssetCache[cacheKey] = assets;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(25);
            bool newFoldout = EditorGUILayout.Foldout(foldout, $"Main Assets ({assets.Count})", true, EditorStyles.foldoutHeader);
            if (GUILayout.Button("↺", GUILayout.Width(22), GUILayout.Height(16)))
                _collectorAssetCache.Remove(cacheKey); // 手动刷新
            EditorGUILayout.EndHorizontal();

            _collectorFoldouts[cacheKey] = newFoldout;

            if (newFoldout)
            {
                Color bg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
                EditorGUILayout.BeginVertical("box");
                GUI.backgroundColor = bg;

                if (assets.Count == 0)
                {
                    EditorGUILayout.LabelField("  (无匹配资源)", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var assetPath in assets)
                    {
                        string address = Path.GetFileNameWithoutExtension(assetPath);
                        EditorGUILayout.LabelField(
                            $"  [{address}]  {assetPath}",
                            EditorStyles.miniLabel);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    // ── 扫描 Collector 匹配的资源 ────────────────────────────
    List<string> ScanCollectorAssets(Collector collector)
    {
        var result = new List<string>();
        if (collector.collectorPath == null) return result;

        string path = AssetDatabase.GetAssetPath(collector.collectorPath);
        if (string.IsNullOrEmpty(path)) return result;

        if (File.Exists(path))
        {
            if (ShouldCollect(path, collector.collectType))
                result.Add(path);
        }
        else if (Directory.Exists(path))
        {
            foreach (string guid in AssetDatabase.FindAssets("", new[] { path }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!Directory.Exists(assetPath) && ShouldCollect(assetPath, collector.collectType))
                    result.Add(assetPath);
            }
        }

        return result;
    }

    static bool ShouldCollect(string assetPath, CollectType collectType)
    {
        if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".meta")) return false;
        switch (collectType)
        {
            case CollectType.CollectAll:         return true;
            case CollectType.CollectPrefab:      return assetPath.EndsWith(".prefab");
            case CollectType.CollectSprite:      return assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".tga");
            case CollectType.CollectSpriteAtlas: return assetPath.EndsWith(".spriteatlas");
            case CollectType.CollectScene:       return assetPath.EndsWith(".unity");
            default: return true;
        }
    }

    // ══════════════════════════════════════════════════════════
    // Tab 1：打 AB
    // ══════════════════════════════════════════════════════════
    void DrawAbTab()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Path", GUILayout.Width(120));
        config.outputPath = EditorGUILayout.TextField(config.outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string p = EditorUtility.OpenFolderPanel("Select Output Path", config.outputPath, "");
            if (!string.IsNullOrEmpty(p))
            {
                config.outputPath = p.StartsWith(Application.dataPath) ? "Assets" + p.Substring(Application.dataPath.Length) : p;
                EditorUtility.SetDirty(config);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CDN Path", GUILayout.Width(120));
        cdnPath = EditorGUILayout.TextField(cdnPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string p = EditorUtility.OpenFolderPanel("Select CDN Path", cdnPath, "");
            if (!string.IsNullOrEmpty(p)) cdnPath = p;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AB Version", GUILayout.Width(120));
        config.version = EditorGUILayout.TextField(config.version);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("DisableWriteTypeTree", GUILayout.Width(150));
        config.disableWriteTypeTree = EditorGUILayout.Toggle(config.disableWriteTypeTree);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enable Encryption", GUILayout.Width(150));
        config.enableEncryption = EditorGUILayout.Toggle(config.enableEncryption);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Platform", GUILayout.Width(120));
        selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup(selectedTarget);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Shader Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ShaderVariant", GUILayout.Width(120));
        config.shaderVariantPath = EditorGUILayout.ObjectField(config.shaderVariantPath, typeof(UnityEngine.Object), false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("自动收集所有被引用的Shader，并与ShaderVariant打包到shaders.bundle", MessageType.Info);

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build AssetBundles", GUILayout.Height(30))) BuildAssetBundles();
        if (GUILayout.Button("Copy to CDN",        GUILayout.Height(30))) CopyToCDN();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Copy Buildin to BundleCache（编辑器跑AB）", GUILayout.Height(28)))
            CopyAbToBundleCache(config.version, false);

        EditorGUILayout.EndVertical();
    }

    // ══════════════════════════════════════════════════════════
    // Tab 2：打安装包
    // ══════════════════════════════════════════════════════════
    void DrawPlayerTab()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Build Player Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Output", GUILayout.Width(120));
        config.buildOutputPath = EditorGUILayout.TextField(config.buildOutputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string p = EditorUtility.OpenFolderPanel("Select Build Output Path", config.buildOutputPath, "");
            if (!string.IsNullOrEmpty(p)) { config.buildOutputPath = p; EditorUtility.SetDirty(config); }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Version", GUILayout.Width(120));
        config.buildVersion = EditorGUILayout.TextField(config.buildVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Copy AB Version", GUILayout.Width(120));
        config.copyAbVersion = EditorGUILayout.TextField(config.copyAbVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build-in Copy", GUILayout.Width(120));
        config.buildInCopyOption = (BuildInCopyOption)EditorGUILayout.EnumPopup(config.buildInCopyOption);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Platform", GUILayout.Width(120));
        selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup(selectedTarget);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Android 设备", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (deviceDisplayNames.Length == 0)
            EditorGUILayout.LabelField("无已连接设备", EditorStyles.miniLabel);
        else
            selectedDeviceIndex = EditorGUILayout.Popup("选择设备", selectedDeviceIndex, deviceDisplayNames);
        if (GUILayout.Button("刷新", GUILayout.Width(50))) RefreshDevices();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包后自动安装", GUILayout.Width(120));
        autoInstallAfterBuild = EditorGUILayout.Toggle(autoInstallAfterBuild);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scene List", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Scene", GUILayout.Width(80), GUILayout.Height(18)))
        {
            config.buildScenes.Add(null);
            EditorUtility.SetDirty(config);
        }
        EditorGUILayout.EndHorizontal();

        sceneScrollPos = EditorGUILayout.BeginScrollView(sceneScrollPos, "box", GUILayout.Height(100));
        for (int i = 0; i < config.buildScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                config.buildScenes.RemoveAt(i);
                EditorUtility.SetDirty(config);
                EditorGUILayout.EndHorizontal();
                continue;
            }
            config.buildScenes[i] = EditorGUILayout.ObjectField(config.buildScenes[i], typeof(UnityEngine.Object), false);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Build Player", GUILayout.Height(35)))
        {
            bool ok = BuildPlayer();
            if (ok && autoInstallAfterBuild && selectedTarget == BuildTarget.Android)
            {
                string platformName = GetPlatformName(selectedTarget);
                string apkPath = Path.Combine(config.buildOutputPath, platformName, PlayerSettings.productName + ".apk");
                string serial = (selectedDeviceIndex >= 0 && selectedDeviceIndex < connectedDevices.Count)
                    ? connectedDevices[selectedDeviceIndex].Split('\t')[0] : "";
                BuildPlayerHelper.InstallAndLaunch(apkPath, serial, PlayerSettings.applicationIdentifier);
            }
        }

        EditorGUILayout.EndVertical();
    }

    // ── 持久化 ────────────────────────────────────────────────
    void DoSaveConfig()
    {
        if (config != null) { EditorUtility.SetDirty(config); AssetDatabase.SaveAssets(); }
        _pendingSave = false;
        _lastSaveTime = EditorApplication.timeSinceStartup;
    }

    // ── Config 管理 ───────────────────────────────────────────
    void CreateNewConfig()
    {
        string path = "Assets/AssetBundleConfig.asset";
        config = CreateInstance<AssetBundleConfig>();
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(config);
        EditorPrefs.SetString(kLastConfig, path);
        selectedGroupIndex = -1;
        _collectorFoldouts.Clear();
        _collectorAssetCache.Clear();
        RefreshConfigList();
    }

    void LoadConfig()
    {
        var configs = AssetDatabase.FindAssets("t:AssetBundleConfig")
            .Select(guid => AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(c => c != null).ToArray();
        if (configs.Length > 0)
        {
            config = configs[0];
            EditorPrefs.SetString(kLastConfig, AssetDatabase.GetAssetPath(config));
        }
        selectedGroupIndex = -1;
    }

    void LoadConfigDialog()
    {
        string path = EditorUtility.OpenFilePanel("Load Config", "Assets", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            config = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(path);
            EditorPrefs.SetString(kLastConfig, path);
            selectedGroupIndex = -1;
            _collectorFoldouts.Clear();
            _collectorAssetCache.Clear();
            RefreshConfigList();
        }
    }

    void SaveConfig() => DoSaveConfig();

    void ExportConfig()
    {
        if (config == null) return;
        string path = EditorUtility.SaveFilePanel("Export Config", "", config.name + ".json", "json");
        if (!string.IsNullOrEmpty(path)) { File.WriteAllText(path, JsonUtility.ToJson(config, true)); Debug.Log("Config exported: " + path); }
    }

    void ImportConfig()
    {
        string path = EditorUtility.OpenFilePanel("Import Config", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            if (config == null) CreateNewConfig();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), config);
            EditorUtility.SetDirty(config);
            selectedGroupIndex = -1;
            _collectorFoldouts.Clear();
            _collectorAssetCache.Clear();
            Debug.Log("Config imported: " + path);
        }
    }

    // ── 平台切换 ──────────────────────────────────────────────
    bool EnsurePlatform()
    {
        if (EditorUserBuildSettings.activeBuildTarget == selectedTarget) return true;

        bool confirm = EditorUtility.DisplayDialog(
            "切换平台",
            $"当前平台: {EditorUserBuildSettings.activeBuildTarget}\n目标平台: {selectedTarget}\n\n需要切换平台，可能需要一段时间，是否继续？",
            "切换", "取消");
        if (!confirm) return false;

        Debug.Log($"[Builder] 切换平台: {EditorUserBuildSettings.activeBuildTarget} → {selectedTarget}");
        bool success = EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildPipeline.GetBuildTargetGroup(selectedTarget), selectedTarget);

        if (!success)
        {
            EditorUtility.DisplayDialog("Error", $"平台切换失败: {selectedTarget}", "OK");
            return false;
        }
        return true;
    }

    // ── 构建操作 ──────────────────────────────────────────────
    void BuildAssetBundles()
    {
        if (config == null) { EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK"); return; }
        if (!EnsurePlatform()) return;
        DoSaveConfig();
        AssetBundleBuilder.Build(config, selectedTarget);
    }

    bool BuildPlayer()
    {
        if (config == null) { EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK"); return false; }
        if (!EnsurePlatform()) return false;
        DoSaveConfig();
        if (config.buildInCopyOption != BuildInCopyOption.None)
        {
            CopyAbToStreamingAssets(config.copyAbVersion, config.buildInCopyOption == BuildInCopyOption.CopyBuildin);
        }
        else
        {
            string destPath = Path.Combine(Application.streamingAssetsPath, "BundlePackTools.zip");
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
                string metaPath = destPath + ".meta";
                if (File.Exists(metaPath)) File.Delete(metaPath);
                AssetDatabase.Refresh();
            }
        }
        string packageType = config.buildInCopyOption switch
        {
            BuildInCopyOption.None        => "Empty",
            BuildInCopyOption.CopyAll     => "Full",
            BuildInCopyOption.CopyBuildin => "Small",
            _                             => "Empty"
        };
        WriteAppConfig(packageType);
        return BuildPlayerHelper.BuildPlayer(config, selectedTarget);
    }

    // ── ADB ───────────────────────────────────────────────────
    void RefreshDevices()
    {
        connectedDevices = BuildPlayerHelper.GetConnectedDevices();
        deviceDisplayNames = connectedDevices.Count > 0 ? connectedDevices.ToArray() : new string[0];
        selectedDeviceIndex = connectedDevices.Count > 0 ? 0 : -1;
        Debug.Log($"[ADB] 找到 {connectedDevices.Count} 台设备");
    }

    // ── 路径 / 拷贝 ───────────────────────────────────────────
    string GetPlatformName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64: return "PC";
            case BuildTarget.Android:             return "Android";
            case BuildTarget.iOS:                 return "iOS";
            default:                              return target.ToString();
        }
    }

    void CopyAbToStreamingAssets(string abVersion, bool onlyBuildin)
    {
        string src  = Path.Combine(config.outputPath, GetPlatformName(selectedTarget), abVersion);
        string dest = Path.Combine(Application.streamingAssetsPath, "BundlePackTools");
        if (!Directory.Exists(src)) { Debug.LogError($"Source not found: {src}"); return; }
        try
        {
            if (Directory.Exists(dest)) Directory.Delete(dest, true);
            Directory.CreateDirectory(dest);
            if (onlyBuildin) CopyBuildinBundles(src, dest); else CopyDirectory(src, dest);
            AssetDatabase.Refresh();
            CompressAndDeleteBundlePackTools(dest);
        }
        catch (System.Exception e) { Debug.LogError($"Failed to copy: {e.Message}"); }
    }

    void CopyAbToBundleCache(string abVersion, bool onlyBuildin)
    {
        string src  = Path.Combine(config.outputPath, GetPlatformName(selectedTarget), abVersion);
        string dest = PathConfig.GetCacheRoot();
        if (!Directory.Exists(src)) { Debug.LogError($"Source not found: {src}"); return; }
        try
        {
            if (Directory.Exists(dest)) Directory.Delete(dest, true);
            Directory.CreateDirectory(dest);
            if (onlyBuildin) CopyBuildinBundles(src, dest); else CopyDirectory(src, dest);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e) { Debug.LogError($"Failed to copy: {e.Message}"); }
    }

    void CopyBuildinBundles(string sourcePath, string destPath)
    {
        string manifestJsonPath = Path.Combine(sourcePath, "DefaultPackage_Manifest.json");
        if (!File.Exists(manifestJsonPath)) { Debug.LogError($"Manifest not found: {manifestJsonPath}"); return; }
        PackageManifest manifest = JsonUtility.FromJson<PackageManifest>(File.ReadAllText(manifestJsonPath));
        var buildinBundles = new HashSet<string>();
        foreach (var b in manifest.BundleList)
            if (b.Tags != null)
                foreach (var tag in b.Tags)
                    if (tag.ToLower() == "buildin") { buildinBundles.Add(b.BundleName); break; }
        foreach (string file in Directory.GetFiles(sourcePath))
        {
            string name = Path.GetFileName(file);
            if (!name.EndsWith(".bundle")) File.Copy(file, Path.Combine(destPath, name), true);
        }
        foreach (string bundle in buildinBundles)
        {
            string src = Path.Combine(sourcePath, bundle);
            if (File.Exists(src)) File.Copy(src, Path.Combine(destPath, bundle), true);
        }
    }

    void CompressAndDeleteBundlePackTools(string dir)
    {
        try
        {
            string zip = dir + ".zip";
            if (File.Exists(zip)) File.Delete(zip);
            ZipFile.CreateFromDirectory(dir, zip, System.IO.Compression.CompressionLevel.Optimal, false);
            Directory.Delete(dir, true);
            string meta = dir + ".meta";
            if (File.Exists(meta)) File.Delete(meta);
            Debug.Log($"Compressed to: {zip}");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e) { Debug.LogError($"Compress failed: {e.Message}"); }
    }

    void WriteAppConfig(string packageType)
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);
        File.WriteAllText(
            Path.Combine(Application.streamingAssetsPath, "AppConfig.json"),
            JsonUtility.ToJson(new AppConfig { PackageType = packageType }, true));
        AssetDatabase.Refresh();
        Debug.Log($"AppConfig written: {packageType}");
    }

    void CopyToCDN()
    {
        if (config == null) { EditorUtility.DisplayDialog("Error", "Please select a config first", "OK"); return; }
        if (string.IsNullOrEmpty(cdnPath)) { EditorUtility.DisplayDialog("Error", "Please set CDN Path", "OK"); return; }
        string platform = GetPlatformName(selectedTarget);
        string src  = Path.Combine(config.outputPath, platform, config.version);
        string dest = Path.Combine(cdnPath, platform, config.version);
        if (!Directory.Exists(src))
        {
            EditorUtility.DisplayDialog("Error", $"Source not found:\n{src}\n\nPlease build AssetBundles first!", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Confirm", $"Copy to CDN?\n\nFrom: {src}\nTo:   {dest}", "Copy", "Cancel")) return;
        try
        {
            if (Directory.Exists(dest)) Directory.Delete(dest, true);
            Directory.CreateDirectory(dest);
            CopyDirectory(src, dest);
            string versionSrc  = Path.Combine(src,  "DefaultPackage_Manifest.version");
            string versionDest = Path.Combine(cdnPath, platform, "DefaultPackage_Manifest.version");
            if (File.Exists(versionSrc)) File.Copy(versionSrc, versionDest, true);
            EditorUtility.DisplayDialog("Success", $"Copied to:\n{dest}", "OK");
        }
        catch (System.Exception e) { EditorUtility.DisplayDialog("Error", e.Message, "OK"); Debug.LogError(e); }
    }

    void CopyDirectory(string src, string dest)
    {
        var dir = new DirectoryInfo(src);
        Directory.CreateDirectory(dest);
        foreach (var f in dir.GetFiles()) f.CopyTo(Path.Combine(dest, f.Name), true);
        foreach (var d in dir.GetDirectories()) CopyDirectory(d.FullName, Path.Combine(dest, d.Name));
    }
}
