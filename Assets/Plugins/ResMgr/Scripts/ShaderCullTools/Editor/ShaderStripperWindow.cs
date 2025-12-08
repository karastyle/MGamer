// ShaderStripperWindow.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ShaderStripperWindow : EditorWindow
{
    private ShaderStripperConfig config;
    private Vector2 scrollPos;
    private Vector2 shaderListScrollPos;
    private int selectedTab = 0;
    private string[] tabNames = { "Shader Rules", "Directory Rules", "Test Shader" };
    private int selectedShaderIndex = -1;
    private int selectedDirectoryIndex = -1;

    private Shader testShader;
    private List<string> testResults = new List<string>();

    [MenuItem("Tools/Shader Stripper")]
    static void ShowWindow()
    {
        var window = GetWindow<ShaderStripperWindow>("Shader Stripper");
        window.minSize = new Vector2(900, 600);
    }

    void OnEnable()
    {
        LoadConfig();
    }

    void OnGUI()
    {
        DrawToolbar();

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        switch (selectedTab)
        {
            case 0:
                DrawShaderRules();
                break;
            case 1:
                DrawDirectoryRules();
                break;
            case 2:
                DrawTestShader();
                break;
        }

        EditorGUILayout.EndScrollView();
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

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    void DrawShaderRules()
    {
        if (config == null)
        {
            EditorGUILayout.HelpBox("Please create or load a config", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Shader Strip Rules", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        DrawShaderList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.7f - 20));
        DrawShaderRuleDetail();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    void DrawShaderList()
    {
        EditorGUILayout.LabelField("Shaders", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

        for (int i = 0; i < config.shaderRules.Count; i++)
        {
            DrawShaderItem(config.shaderRules[i], i);
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Add Shader", GUILayout.Height(25)))
        {
            config.shaderRules.Add(new ShaderRule());
            EditorUtility.SetDirty(config);
        }
    }

    void DrawShaderItem(ShaderRule shaderRule, int index)
    {
        bool isSelected = selectedShaderIndex == index;

        Color originalColor = GUI.backgroundColor;
        if (isSelected)
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            config.shaderRules.RemoveAt(index);
            if (selectedShaderIndex == index)
                selectedShaderIndex = -1;
            EditorUtility.SetDirty(config);
            GUI.backgroundColor = originalColor;
            return;
        }

        string displayName = shaderRule.shader != null ? shaderRule.shader.name : "<None>";
        if (GUILayout.Button(displayName, EditorStyles.label))
        {
            selectedShaderIndex = index;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        GUI.backgroundColor = originalColor;
    }

    void DrawShaderRuleDetail()
    {
        if (selectedShaderIndex < 0 || selectedShaderIndex >= config.shaderRules.Count)
        {
            EditorGUILayout.HelpBox("Select a shader to configure", MessageType.Info);
            return;
        }

        var shaderRule = config.shaderRules[selectedShaderIndex];

        EditorGUILayout.LabelField("Shader Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        shaderRule.shader = EditorGUILayout.ObjectField("Shader", shaderRule.shader, typeof(Shader), false) as Shader;
        EditorGUILayout.EndVertical();

        if (shaderRule.shader == null)
        {
            EditorGUILayout.HelpBox("Please select a shader", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Strip Rules", EditorStyles.boldLabel);

        List<string> keywords = GetShaderKeywords(shaderRule.shader);

        for (int i = 0; i < shaderRule.rules.Count; i++)
        {
            DrawShaderStripRule(shaderRule.rules[i], i, keywords, () => shaderRule.rules.RemoveAt(i));
        }

        if (GUILayout.Button("Add Rule", GUILayout.Height(30)))
        {
            shaderRule.rules.Add(new StripRule { ruleName = "Rule " + shaderRule.rules.Count });
            EditorUtility.SetDirty(config);
        }
    }

    void DrawShaderStripRule(StripRule rule, int index, List<string> keywords, System.Action onRemove)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            onRemove?.Invoke();
            EditorUtility.SetDirty(config);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        rule.ruleName = EditorGUILayout.TextField("Rule Name", rule.ruleName);
        rule.enabled = EditorGUILayout.Toggle("Enabled", rule.enabled, GUILayout.Width(80));

        EditorGUILayout.EndHorizontal();

        rule.ruleType = (RuleType)EditorGUILayout.EnumPopup("Rule Type", rule.ruleType);

        if (keywords.Count == 0)
        {
            EditorGUILayout.HelpBox("No keywords found in shader", MessageType.Warning);
        }
        else
        {
            if (rule.ruleType == RuleType.SingleKeyword)
            {
                int selectedIndex = keywords.IndexOf(rule.keyword1);
                if (selectedIndex < 0) selectedIndex = 0;

                selectedIndex = EditorGUILayout.Popup("Keyword", selectedIndex, keywords.ToArray());
                rule.keyword1 = keywords[selectedIndex];

                EditorGUILayout.HelpBox("Strip if contains: " + rule.keyword1, MessageType.Info);
            }
            else if (rule.ruleType == RuleType.KeywordCombination)
            {
                int selectedIndex1 = keywords.IndexOf(rule.keyword1);
                if (selectedIndex1 < 0) selectedIndex1 = 0;

                int selectedIndex2 = keywords.IndexOf(rule.keyword2);
                if (selectedIndex2 < 0) selectedIndex2 = 0;

                selectedIndex1 = EditorGUILayout.Popup("Keyword 1", selectedIndex1, keywords.ToArray());
                rule.keyword1 = keywords[selectedIndex1];

                selectedIndex2 = EditorGUILayout.Popup("Keyword 2", selectedIndex2, keywords.ToArray());
                rule.keyword2 = keywords[selectedIndex2];

                EditorGUILayout.HelpBox($"Strip if contains both: {rule.keyword1} AND {rule.keyword2}",
                    MessageType.Info);
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    List<string> GetShaderKeywords(Shader shader)
    {
        if (shader == null) return new List<string>();

        HashSet<string> keywords = new HashSet<string>();

        string path = AssetDatabase.GetAssetPath(shader);
        if (string.IsNullOrEmpty(path)) return keywords.ToList();

        try
        {
            string shaderCode = System.IO.File.ReadAllText(path);

            var pragmaPattern = @"#pragma\s+(?:multi_compile|shader_feature)(?:_local)?\s+(.+)";
            var matches = System.Text.RegularExpressions.Regex.Matches(shaderCode, pragmaPattern);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string line = match.Groups[1].Value;

                    int commentIndex = line.IndexOf("//");
                    if (commentIndex >= 0)
                    {
                        line = line.Substring(0, commentIndex);
                    }

                    string[] kws = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string kw in kws)
                    {
                        string cleanKw = kw.Trim();
                        if (!string.IsNullOrEmpty(cleanKw) && cleanKw != "_")
                        {
                            keywords.Add(cleanKw);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse shader keywords: {e.Message}");
        }

        if (keywords.Count == 0)
        {
            keywords.Add("NO_KEYWORDS_FOUND");
        }

        return keywords.OrderBy(k => k).ToList();
    }

    void DrawDirectoryRules()
    {
        if (config == null)
        {
            EditorGUILayout.HelpBox("Please create or load a config", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Directory Rules", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        DrawDirectoryList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.7f - 20));
        DrawDirectoryDetail();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    void DrawDirectoryList()
    {
        EditorGUILayout.LabelField("Directories", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

        for (int i = 0; i < config.directoryRules.Count; i++)
        {
            DrawDirectoryItem(config.directoryRules[i], i);
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Add Directory", GUILayout.Height(25)))
        {
            config.directoryRules.Add(new DirectoryRule { directoryName = "Directory " + config.directoryRules.Count });
            EditorUtility.SetDirty(config);
        }
    }

    void DrawDirectoryItem(DirectoryRule dirRule, int index)
    {
        bool isSelected = selectedDirectoryIndex == index;

        Color originalColor = GUI.backgroundColor;
        if (isSelected)
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            config.directoryRules.RemoveAt(index);
            if (selectedDirectoryIndex == index)
                selectedDirectoryIndex = -1;
            EditorUtility.SetDirty(config);
            GUI.backgroundColor = originalColor;
            return;
        }

        if (GUILayout.Button(dirRule.directoryName, EditorStyles.label))
        {
            selectedDirectoryIndex = index;
        }

        dirRule.enabled = EditorGUILayout.Toggle(dirRule.enabled, GUILayout.Width(20));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        GUI.backgroundColor = originalColor;
    }

    void DrawDirectoryDetail()
    {
        if (selectedDirectoryIndex < 0 || selectedDirectoryIndex >= config.directoryRules.Count)
        {
            EditorGUILayout.HelpBox("Select a directory to configure", MessageType.Info);
            return;
        }

        var dirRule = config.directoryRules[selectedDirectoryIndex];

        EditorGUILayout.LabelField("Directory Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        dirRule.directoryName = EditorGUILayout.TextField("Name", dirRule.directoryName);
        dirRule.directory =
            EditorGUILayout.ObjectField("Directory", dirRule.directory, typeof(UnityEngine.Object), false);
        dirRule.enabled = EditorGUILayout.Toggle("Enabled", dirRule.enabled);
        EditorGUILayout.EndVertical();

        if (dirRule.directory != null)
        {
            EditorGUILayout.Space(5);
            DrawShaderListInDirectory(dirRule);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Strip Rules", EditorStyles.boldLabel);

        for (int i = 0; i < dirRule.rules.Count; i++)
        {
            DrawDirectoryStripRule(dirRule.rules[i], i, () => dirRule.rules.RemoveAt(i));
        }

        if (GUILayout.Button("Add Rule", GUILayout.Height(30)))
        {
            dirRule.rules.Add(new StripRule { ruleName = "Rule " + dirRule.rules.Count });
            EditorUtility.SetDirty(config);
        }
    }

    void DrawShaderListInDirectory(DirectoryRule dirRule)
    {
        string dirPath = AssetDatabase.GetAssetPath(dirRule.directory);
        if (string.IsNullOrEmpty(dirPath))
        {
            EditorGUILayout.HelpBox("Invalid directory", MessageType.Warning);
            return;
        }

        string[] shaderGuids = AssetDatabase.FindAssets("t:Shader", new[] { dirPath });

        EditorGUILayout.LabelField($"Shaders in Directory ({shaderGuids.Length})", EditorStyles.boldLabel);

        shaderListScrollPos = EditorGUILayout.BeginScrollView(shaderListScrollPos, "box", GUILayout.Height(150));

        foreach (string guid in shaderGuids)
        {
            string shaderPath = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

            if (shader != null)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(shader.name, GUILayout.Width(300));

                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(shader);
                    Selection.activeObject = shader;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawDirectoryStripRule(StripRule rule, int index, System.Action onRemove)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            onRemove?.Invoke();
            EditorUtility.SetDirty(config);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        rule.ruleName = EditorGUILayout.TextField("Rule Name", rule.ruleName);
        rule.enabled = EditorGUILayout.Toggle("Enabled", rule.enabled, GUILayout.Width(80));

        EditorGUILayout.EndHorizontal();

        rule.ruleType = (RuleType)EditorGUILayout.EnumPopup("Rule Type", rule.ruleType);

        if (rule.ruleType == RuleType.SingleKeyword)
        {
            rule.keyword1 = EditorGUILayout.TextField("Keyword", rule.keyword1);
            EditorGUILayout.HelpBox("Strip if contains: " + rule.keyword1, MessageType.Info);
        }
        else if (rule.ruleType == RuleType.KeywordCombination)
        {
            rule.keyword1 = EditorGUILayout.TextField("Keyword 1", rule.keyword1);
            rule.keyword2 = EditorGUILayout.TextField("Keyword 2", rule.keyword2);
            EditorGUILayout.HelpBox($"Strip if contains both: {rule.keyword1} AND {rule.keyword2}", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    void DrawTestShader()
    {
        if (config == null)
        {
            EditorGUILayout.HelpBox("Please create or load a config", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Test Shader Stripping", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        testShader = EditorGUILayout.ObjectField("Test Shader", testShader, typeof(Shader), false) as Shader;

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Analyze Shader", GUILayout.Height(40)))
        {
            AnalyzeShader();
        }

        EditorGUILayout.EndVertical();

        if (testResults.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Analysis Results:", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            foreach (var result in testResults)
            {
                EditorGUILayout.HelpBox(result, MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }
    }

    void AnalyzeShader()
    {
        testResults.Clear();

        if (testShader == null)
        {
            testResults.Add("Please select a shader to test");
            return;
        }

        string shaderPath = AssetDatabase.GetAssetPath(testShader);
        bool hasMatches = false;

        foreach (var shaderRule in config.shaderRules)
        {
            if (shaderRule.shader == testShader)
            {
                foreach (var rule in shaderRule.rules)
                {
                    if (!rule.enabled) continue;

                    string ruleInfo = GetRuleDescription(rule);
                    testResults.Add($"[Shader Rule] {rule.ruleName}: {ruleInfo}");
                    hasMatches = true;
                }
            }
        }

        foreach (var dirRule in config.directoryRules)
        {
            if (!dirRule.enabled) continue;
            if (dirRule.directory == null) continue;

            string dirPath = AssetDatabase.GetAssetPath(dirRule.directory);
            if (string.IsNullOrEmpty(dirPath)) continue;

            if (shaderPath.StartsWith(dirPath))
            {
                foreach (var rule in dirRule.rules)
                {
                    if (!rule.enabled) continue;

                    string ruleInfo = GetRuleDescription(rule);
                    testResults.Add($"[{dirRule.directoryName}] {rule.ruleName}: {ruleInfo}");
                    hasMatches = true;
                }
            }
        }

        if (!hasMatches)
        {
            testResults.Add("No stripping rules will be applied to this shader");
        }
    }

    string GetRuleDescription(StripRule rule)
    {
        if (rule.ruleType == RuleType.SingleKeyword)
        {
            return "Strip if contains: " + rule.keyword1;
        }
        else
        {
            return $"Strip if contains both: {rule.keyword1} AND {rule.keyword2}";
        }
    }

    void CreateNewConfig()
    {
        config = CreateInstance<ShaderStripperConfig>();
        string path = "Assets/ShaderStripperConfig.asset";
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(config);
        selectedShaderIndex = -1;
        selectedDirectoryIndex = -1;
    }

    void LoadConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:ShaderStripperConfig");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<ShaderStripperConfig>(path);
        }

        selectedShaderIndex = -1;
        selectedDirectoryIndex = -1;
    }

    void LoadConfigDialog()
    {
        string path = EditorUtility.OpenFilePanel("Load Config", "Assets", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            config = AssetDatabase.LoadAssetAtPath<ShaderStripperConfig>(path);
            selectedShaderIndex = -1;
            selectedDirectoryIndex = -1;
        }
    }

    //记得使用clean build来重新编译shader
    void SaveConfig()
    {
        if (config != null)
        {
            config.IncrementVersion();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>[Config Saved]</color> Version: {config.configVersion}");
            Debug.Log("Please use Clean Build to recompile shaders with the new config.");
        }
    }
}