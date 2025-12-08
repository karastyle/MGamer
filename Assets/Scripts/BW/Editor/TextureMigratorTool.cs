using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class TextureMigratorTool : EditorWindow
{
    private string targetDirectory = "";
    private Vector2 scrollPos;
    private List<string> logs = new List<string>();

    [MenuItem("Tools/资源工具/贴图引用迁移工具 (Texture Migrator)")]
    public static void ShowWindow()
    {
        GetWindow<TextureMigratorTool>("贴图迁移");
    }

    private void OnGUI()
    {
        GUILayout.Label("配置与操作", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        targetDirectory = EditorGUILayout.TextField("根目录 (a):", targetDirectory);
        if (GUILayout.Button("选择目录", GUILayout.Width(100)))
        {
            string path = EditorUtility.OpenFolderPanel("选择根目录", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // 转换为相对路径 Assets/...
                if (path.StartsWith(Application.dataPath))
                {
                    targetDirectory = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择项目Assets目录下的文件夹", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("逻辑说明：\n1. 遍历根目录下所有Prefab。\n2. 若Prefab引用的贴图在根目录之外。\n3. 在Prefab所在目录的平级目录寻找或创建 'Atlas' 文件夹。\n4. 复制贴图并在Prefab中修改引用。", MessageType.Info);

        if (GUILayout.Button("开始扫描并迁移", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                EditorUtility.DisplayDialog("错误", "目录无效", "确定");
                return;
            }
            MigrateTextures();
        }

        GUILayout.Space(10);
        GUILayout.Label("处理日志:", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var log in logs)
        {
            GUILayout.Label(log);
        }
        EditorGUILayout.EndScrollView();
    }

    private void MigrateTextures()
    {
        logs.Clear();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { targetDirectory });
        int total = prefabGuids.Length;
        int count = 0;

        try
        {
            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                count++;
                EditorUtility.DisplayProgressBar("处理中", $"正在分析: {prefabPath}", (float)count / total);

                ProcessPrefab(prefabPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"处理出错: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            logs.Add("------ 处理完成 ------");
        }
    }

    private void ProcessPrefab(string prefabPath)
    {
        // 获取该Prefab的所有依赖
        string[] dependencies = AssetDatabase.GetDependencies(prefabPath, false);
        
        // 筛选出贴图依赖 (Texture, Sprite等)
        var texturePaths = dependencies.Where(p => 
            p != prefabPath && // 排除自己
            IsTexture(p) && 
            !p.StartsWith(targetDirectory) // 关键点：引用了配置目录a之外的资源
        ).ToList();

        if (texturePaths.Count == 0) return;

        // 开始修改Prefab
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
        bool isModified = false;

        // 获取目标Atlas目录路径
        // 逻辑：Prefab目录: Assets/A/B/C -> Parent: Assets/A/B -> Sibling: Assets/A/B/Atlas
        string prefabDir = Path.GetDirectoryName(prefabPath);
        string parentDir = Path.GetDirectoryName(prefabDir); 
        // 如果Prefab就在Assets根目录下，parentDir可能为空，需做容错，但通常都在Assets下
        string atlasDir = Path.Combine(parentDir, "Atlas").Replace("\\", "/");

        foreach (string sourceTexPath in texturePaths)
        {
            // 1. 确保Atlas目录存在
            if (!Directory.Exists(atlasDir))
            {
                Directory.CreateDirectory(atlasDir);
                AssetDatabase.Refresh(); // 刷新以确保Unity识别新文件夹
            }

            string fileName = Path.GetFileName(sourceTexPath);
            string destTexPath = $"{atlasDir}/{fileName}";

            // 2. 复制贴图 (如果目标不存在)
            // 即使目标已存在同名文件，我们也认为它就是要用的那个（共享资源）
            if (!File.Exists(destTexPath))
            {
                AssetDatabase.CopyAsset(sourceTexPath, destTexPath);
                logs.Add($"[复制] {Path.GetFileName(sourceTexPath)} -> {atlasDir}");
            }

            // 3. 替换Prefab内部引用
            // 加载源资源对象和目标资源对象
            Object sourceObj = AssetDatabase.LoadAssetAtPath<Object>(sourceTexPath);
            Object destObj = AssetDatabase.LoadAssetAtPath<Object>(destTexPath);

            if (destObj != null)
            {
                if (ReplaceReferencesInGameObject(prefabContents, sourceObj, destObj, sourceTexPath, destTexPath))
                {
                    isModified = true;
                }
            }
        }

        if (isModified)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            logs.Add($"[修改引用] Prefab: {prefabPath}");
        }

        PrefabUtility.UnloadPrefabContents(prefabContents);
    }

    /// <summary>
    /// 递归替换GameObject及其子物体上的组件引用
    /// </summary>
    private bool ReplaceReferencesInGameObject(GameObject go, Object sourceObj, Object destObj, string srcPath, string destPath)
    {
        bool changed = false;
        Component[] components = go.GetComponentsInChildren<Component>(true);

        foreach (var comp in components)
        {
            if (comp == null) continue;

            SerializedObject so = new SerializedObject(comp);
            SerializedProperty iterator = so.GetIterator();

            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // 情况A: 属性引用的是 Texture2D
                    if (iterator.objectReferenceValue == sourceObj)
                    {
                        iterator.objectReferenceValue = destObj;
                        changed = true;
                    }
                    // 情况B: 属性引用的是 Sprite
                    // Sprite是Texture的子资源，如果源是Sprite，我们需要去新Texture里找对应的Sprite
                    else if (iterator.objectReferenceValue is Sprite oldSprite)
                    {
                        // 检查这个Sprite是否属于我们要替换的那个Texture路径
                        if (AssetDatabase.GetAssetPath(oldSprite) == srcPath)
                        {
                            // 在新路径加载对应的Sprite
                            Sprite newSprite = AssetDatabase.LoadAllAssetsAtPath(destPath)
                                .OfType<Sprite>()
                                .FirstOrDefault(s => s.name == oldSprite.name);

                            if (newSprite != null)
                            {
                                iterator.objectReferenceValue = newSprite;
                                changed = true;
                            }
                        }
                    }
                }
            }

            if (changed)
            {
                so.ApplyModifiedProperties();
            }
        }
        return changed;
    }

    private bool IsTexture(string path)
    {
        // 简单判断扩展名，也可以通过 AssetImporter.GetAtPath(path) is TextureImporter 来判断
        string ext = Path.GetExtension(path).ToLower();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".psd";
    }
}