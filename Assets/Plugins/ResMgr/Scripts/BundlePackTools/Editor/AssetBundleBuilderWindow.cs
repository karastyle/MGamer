// AssetBundleBuilderWindow.cs

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.IO.Compression;

public class AssetBundleBuilderWindow : EditorWindow
{
    private AssetBundleConfig config;
    private Vector2 groupScrollPos;
    private Vector2 collectorScrollPos;
    private Vector2 sceneScrollPos;
    private BuildTarget selectedTarget = BuildTarget.StandaloneWindows64;
    private string configPath = "Assets/AssetBundleConfig.asset";
    private int selectedGroupIndex = -1;
    private string cdnPath = "";

    [MenuItem("Tools/AssetBundle Builder")]
    static void ShowWindow()
    {
        var window = GetWindow<AssetBundleBuilderWindow>("AssetBundle Builder");
        window.minSize = new Vector2(900, 600);
    }

    void OnEnable()
    {
        LoadConfig();
        cdnPath = EditorPrefs.GetString("AssetBundleBuilder_CDNPath", "");
    }

    void OnDisable()
    {
        EditorPrefs.SetString("AssetBundleBuilder_CDNPath", cdnPath);
    }

    void OnGUI()
    {
        DrawToolbar();
        DrawMainLayout();
    }

    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("New Config", EditorStyles.toolbarButton))
            CreateNewConfig();
        
        if (GUILayout.Button("Load Config", EditorStyles.toolbarButton))
            LoadConfigDialog();
        
        if (GUILayout.Button("Save Config", EditorStyles.toolbarButton))
            SaveConfig();
        
        if (GUILayout.Button("Export", EditorStyles.toolbarButton))
            ExportConfig();
        
        if (GUILayout.Button("Import", EditorStyles.toolbarButton))
            ImportConfig();
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
    }

    void DrawMainLayout()
    {
        if (config == null)
        {
            EditorGUILayout.HelpBox("Please create or load a config", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        
        DrawGroupsPanel();
        DrawCollectorsPanel();
        
        EditorGUILayout.EndHorizontal();

        DrawBuildSection();
    }

    void DrawGroupsPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        
        EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
        
        groupScrollPos = EditorGUILayout.BeginScrollView(groupScrollPos, "box", GUILayout.ExpandHeight(true));
        
        for (int i = 0; i < config.collectorGroups.Count; i++)
        {
            DrawGroupItem(config.collectorGroups[i], i);
        }
        
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
        
        Color originalColor = GUI.backgroundColor;
        if (isSelected)
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);

        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            config.collectorGroups.RemoveAt(index);
            if (selectedGroupIndex == index)
                selectedGroupIndex = -1;
            EditorUtility.SetDirty(config);
            GUI.backgroundColor = originalColor;
            return;
        }
        
        if (GUILayout.Button(group.groupName, EditorStyles.label))
        {
            selectedGroupIndex = index;
        }
        
        group.active = EditorGUILayout.Toggle(group.active, GUILayout.Width(20));
        
        EditorGUILayout.EndHorizontal();
        
        if (isSelected)
        {
            group.groupName = EditorGUILayout.TextField("Name", group.groupName);
        }
        
        EditorGUILayout.EndVertical();
        
        GUI.backgroundColor = originalColor;
    }

    void DrawCollectorsPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.7f - 20));
        
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
        {
            DrawCollectorItem(group.collectors[i], i, group);
        }
        
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
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            group.collectors.RemoveAt(index);
            EditorUtility.SetDirty(config);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        
        collector.collectorPath = EditorGUILayout.ObjectField(collector.collectorPath, typeof(UnityEngine.Object), false);
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(25);
        
        EditorGUILayout.LabelField("Pack Rule", GUILayout.Width(70));
        collector.packRule = (PackRule)EditorGUILayout.EnumPopup(collector.packRule, GUILayout.Width(150));
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Collect Type", GUILayout.Width(80));
        collector.collectType = (CollectType)EditorGUILayout.EnumPopup(collector.collectType, GUILayout.Width(150));
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(25);
        
        EditorGUILayout.LabelField("Asset Tag", GUILayout.Width(70));
        collector.assetTag = (AssetTag)EditorGUILayout.EnumPopup(collector.assetTag, GUILayout.Width(480));
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    void DrawBuildSection()
    {
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Path", GUILayout.Width(80));
        config.outputPath = EditorGUILayout.TextField(config.outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Path", config.outputPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                    config.outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                else
                    config.outputPath = path;
                EditorUtility.SetDirty(config);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CDN Path", GUILayout.Width(80));
        cdnPath = EditorGUILayout.TextField(cdnPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select CDN Path", cdnPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                cdnPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AB Version", GUILayout.Width(80));
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
        EditorGUILayout.LabelField("Compress Res to Zip", GUILayout.Width(150)); 
        config.compressResToZip = EditorGUILayout.Toggle(config.compressResToZip);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Platform", GUILayout.Width(80));
        selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup(selectedTarget);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Shader Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ShaderVariant", GUILayout.Width(100));
        config.shaderVariantPath = EditorGUILayout.ObjectField(config.shaderVariantPath, typeof(UnityEngine.Object), false);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox("自动收集所有被引用的Shader,并与ShaderVariant打包到shaders.bundle", MessageType.Info);

        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Build AssetBundles", GUILayout.Height(30)))
        {
            BuildAssetBundles();
        }
        
        if (GUILayout.Button("Copy to CDN", GUILayout.Height(30)))
        {
            CopyToCDN();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Copy All to StreamingAssets", GUILayout.Height(30)))
        {
            CopyAllToStreamingAssets();
        }
        
        if (GUILayout.Button("Copy Buildin to StreamingAssets", GUILayout.Height(30)))
        {
            CopyBuildinToStreamingAssets();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Build Player Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Output", GUILayout.Width(80));
        config.buildOutputPath = EditorGUILayout.TextField(config.buildOutputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Build Output Path", config.buildOutputPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                config.buildOutputPath = path;
                EditorUtility.SetDirty(config);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Version", GUILayout.Width(80));
        config.buildVersion = EditorGUILayout.TextField(config.buildVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Copy AB Version", GUILayout.Width(100));
        config.copyAbVersion = EditorGUILayout.TextField(config.copyAbVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build-in Copy", GUILayout.Width(100));
        config.buildInCopyOption = (BuildInCopyOption)EditorGUILayout.EnumPopup(config.buildInCopyOption);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

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
            BuildPlayer();
        }
        
        EditorGUILayout.EndVertical();
    }

    void CreateNewConfig()
    {
        config = CreateInstance<AssetBundleConfig>();
        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(config);
        selectedGroupIndex = -1;
    }

    void LoadConfig()
    {
        config = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(configPath);
        if (config == null)
        {
            var configs = AssetDatabase.FindAssets("t:AssetBundleConfig")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            if (configs.Length > 0)
                config = configs[0];
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
            configPath = path;
            selectedGroupIndex = -1;
        }
    }

    void SaveConfig()
    {
        if (config != null)
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("Config saved");
        }
    }

    void ExportConfig()
    {
        if (config == null) return;

        string path = EditorUtility.SaveFilePanel("Export Config", "", config.name + ".json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(path, json);
            Debug.Log("Config exported to: " + path);
        }
    }

    void ImportConfig()
    {
        string path = EditorUtility.OpenFilePanel("Import Config", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);
            if (config == null)
                CreateNewConfig();
            
            JsonUtility.FromJsonOverwrite(json, config);
            EditorUtility.SetDirty(config);
            selectedGroupIndex = -1;
            Debug.Log("Config imported from: " + path);
        }
    }

    void BuildAssetBundles()
    {
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK");
            return;
        }

        SaveConfig();
        AssetBundleBuilder.Build(config, selectedTarget);
    }

    void CopyAllToStreamingAssets()
    {
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK");
            return;
        }

        CopyAbToStreamingAssets(config.version, false);
    }

    void CopyBuildinToStreamingAssets()
    {
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK");
            return;
        }

        CopyAbToStreamingAssets(config.version, true);
    }

    void CopyAbToStreamingAssets(string abVersion, bool onlyBuildin)
    {
        string platformName = GetPlatformName(selectedTarget);
        string sourcePath = Path.Combine(config.outputPath, platformName, abVersion);
        string destPath = Path.Combine(Application.streamingAssetsPath, "BundlePackTools");

        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError($"Source path not found: {sourcePath}");
            return;
        }

        try
        {
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }

            Directory.CreateDirectory(destPath);

            if (onlyBuildin)
            {
                CopyBuildinBundles(sourcePath, destPath);
            }
            else
            {
                CopyDirectory(sourcePath, destPath);
            }

            AssetDatabase.Refresh();
            
            string copyType = onlyBuildin ? "Buildin" : "All";
            Debug.Log($"{copyType} AssetBundles copied to: {destPath}");

            // ✅ 如果勾选了压缩选项，压缩并删除文件夹
            if (config.compressResToZip)
            {
                CompressAndDeleteBundlePackTools(destPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to copy AssetBundles: {e.Message}");
        }
    }

    void CopyBuildinBundles(string sourcePath, string destPath)
    {
        string manifestJsonPath = Path.Combine(sourcePath, "DefaultPackage_Manifest.json");
        if (!File.Exists(manifestJsonPath))
        {
            Debug.LogError($"Manifest file not found: {manifestJsonPath}");
            return;
        }

        string json = File.ReadAllText(manifestJsonPath);
        PackageManifest manifest = JsonUtility.FromJson<PackageManifest>(json);

        HashSet<string> buildinBundles = new HashSet<string>();
        foreach (var bundleInfo in manifest.BundleList)
        {
            if (bundleInfo.Tags != null)
            {
                foreach (var tag in bundleInfo.Tags)
                {
                    if (tag.ToLower() == "buildin")
                    {
                        buildinBundles.Add(bundleInfo.BundleName);
                        break;
                    }
                }
            }
        }

        Debug.Log($"Found {buildinBundles.Count} buildin bundles");

        string[] allFiles = Directory.GetFiles(sourcePath);
        foreach (string file in allFiles)
        {
            string fileName = Path.GetFileName(file);
            if (!fileName.EndsWith(".bundle"))
            {
                string destFile = Path.Combine(destPath, fileName);
                File.Copy(file, destFile, true);
            }
        }

        foreach (string bundleName in buildinBundles)
        {
            string sourceFile = Path.Combine(sourcePath, bundleName);
            if (File.Exists(sourceFile))
            {
                string destFile = Path.Combine(destPath, bundleName);
                File.Copy(sourceFile, destFile, true);
            }
        }
    }


    // ✅ 新增：压缩BundlePackTools并删除文件夹
    void CompressAndDeleteBundlePackTools(string bundlePackToolsPath)
    {
        try
        {
            string zipPath = bundlePackToolsPath + ".zip";
        
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(bundlePackToolsPath, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
        
            Debug.Log($"BundlePackTools compressed to: {zipPath}");

            // 删除原文件夹
            Directory.Delete(bundlePackToolsPath, true);
        
            // 删除对应的.meta文件
            string metaPath = bundlePackToolsPath + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
        
            Debug.Log($"BundlePackTools folder and meta deleted: {bundlePackToolsPath}");
        
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to compress BundlePackTools: {e.Message}");
        }
    }

    void WriteAppConfig(string packageType)
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        AppConfig appConfig = new AppConfig
        {
            PackageType = packageType
        };

        string json = JsonUtility.ToJson(appConfig, true);
        string configPath = Path.Combine(Application.streamingAssetsPath, "AppConfig.json");
        File.WriteAllText(configPath, json);
        
        AssetDatabase.Refresh();
        
        Debug.Log($"AppConfig written: PackageType = {packageType}");
    }

    void CopyToCDN()
    {
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK");
            return;
        }

        if (string.IsNullOrEmpty(cdnPath))
        {
            EditorUtility.DisplayDialog("Error", "Please set CDN Path first", "OK");
            return;
        }

        string platformName = GetPlatformName(selectedTarget);
        string sourcePath = Path.Combine(config.outputPath, platformName, config.version);
        string destPath = Path.Combine(cdnPath, platformName, config.version);
        string rootVersionPath = Path.Combine(cdnPath, platformName);

        if (!Directory.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog("Error", $"Source path not found: {sourcePath}\n\nPlease build AssetBundles first!", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Confirm", 
            $"Copy AssetBundles to CDN?\n\nFrom: {sourcePath}\nTo: {destPath}", 
            "Copy", "Cancel");

        if (!confirm)
            return;

        try
        {
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }

            Directory.CreateDirectory(destPath);

            CopyDirectory(sourcePath, destPath);
            
            string sourceVersionFile = Path.Combine(sourcePath, "DefaultPackage_Manifest.version");
            string destVersionFile = Path.Combine(rootVersionPath, "DefaultPackage_Manifest.version");
            if (File.Exists(sourceVersionFile))
            {
                File.Copy(sourceVersionFile, destVersionFile, true);
            }
            
            EditorUtility.DisplayDialog("Success", 
                $"AssetBundles copied to CDN!\n\nLocation: {destPath}", 
                "OK");
            
            Debug.Log($"AssetBundles copied to CDN: {destPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to copy AssetBundles:\n\n{e.Message}", "OK");
            Debug.LogError(e);
        }
    }

    void BuildPlayer()
    {
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select or create a config first", "OK");
            return;
        }

        SaveConfig();
        
        if (config.buildInCopyOption != BuildInCopyOption.None)
        {
            CopyAbToStreamingAssets(config.copyAbVersion, config.buildInCopyOption == BuildInCopyOption.CopyBuildin);
        }
        else
        {
            // 清理StreamingAssets中的BundlePackTools
            string destPath = Path.Combine(Application.streamingAssetsPath, "BundlePackTools.zip");
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
                string metaPath = destPath + ".meta";
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
                AssetDatabase.Refresh();
                Debug.Log("Cleared BundlePackTools from StreamingAssets");
            }
        }
        
        string packageType = "Empty";
        switch (config.buildInCopyOption)
        {
            case BuildInCopyOption.None:
                packageType = "Empty";
                break;
            case BuildInCopyOption.CopyAll:
                packageType = "Full";
                break;
            case BuildInCopyOption.CopyBuildin:
                packageType = "Small";
                break;
        }
        
        WriteAppConfig(packageType);
        
        BuildPlayerHelper.BuildPlayer(config, selectedTarget);
    }

    string GetPlatformName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "PC";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            default:
                return target.ToString();
        }
    }

    void CopyDirectory(string sourceDir, string destDir)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);
        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestDir = Path.Combine(destDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestDir);
        }
    }
}