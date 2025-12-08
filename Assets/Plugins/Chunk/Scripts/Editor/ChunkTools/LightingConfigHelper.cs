using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// 光照配置辅助类
/// 负责创建、更新和应用全局光照配置
/// </summary>
public static class LightingConfigHelper
{
    /// <summary>
    /// 创建或更新全局光照配置Prefab
    /// </summary>
    public static bool CreateOrUpdateGlobalLightingPrefab(string prefabPath, out GameObject prefab)
    {
        prefab = null;

        // 查找场景中的平行光
        Light[] lights = Object.FindObjectsOfType<Light>();
        Light directionalLight = null;

        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                directionalLight = light;
                break;
            }
        }

        if (directionalLight == null)
        {
            EditorUtility.DisplayDialog("错误",
                "场景中未找到平行光！\n请先在场景中创建Directional Light。",
                "确定");
            return false;
        }

        // 创建Prefab根节点
        GameObject lightingRoot = new GameObject("GlobalLighting");

        // 复制平行光到根节点
        GameObject lightCopy = new GameObject(directionalLight.name);
        lightCopy.transform.SetParent(lightingRoot.transform);

        Light newLight = lightCopy.AddComponent<Light>();
        newLight.type = LightType.Directional;
        newLight.color = directionalLight.color;
        newLight.intensity = directionalLight.intensity;
        newLight.shadows = directionalLight.shadows;
        newLight.shadowStrength = directionalLight.shadowStrength;
        newLight.shadowResolution = directionalLight.shadowResolution;
        newLight.shadowBias = directionalLight.shadowBias;
        newLight.shadowNormalBias = directionalLight.shadowNormalBias;
        newLight.shadowNearPlane = directionalLight.shadowNearPlane;

        lightCopy.transform.rotation = directionalLight.transform.rotation;

        // 创建设置存储节点
        GameObject settingsObj = new GameObject("LightmapSettings");
        settingsObj.transform.SetParent(lightingRoot.transform);
        
        // 添加LightmapSettingsStorage组件
        settingsObj.AddComponent<LightmapSettingsStorage>();

        // 创建Prefab目录
        string prefabDir = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabDir))
        {
            Directory.CreateDirectory(prefabDir);
            AssetDatabase.Refresh();
        }

        // 检查是否已存在Prefab
        bool isUpdate = File.Exists(prefabPath);

        // 保存为Prefab
        PrefabUtility.SaveAsPrefabAsset(lightingRoot, prefabPath);
        Object.DestroyImmediate(lightingRoot);

        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        string message = isUpdate ? "更新" : "创建";
        EditorUtility.DisplayDialog("完成",
            $"全局光照配置已{message}！\n\n" +
            $"保存位置: {prefabPath}\n\n" +
            "✅ 已保存:\n" +
            $"• 平行光方向: {directionalLight.transform.eulerAngles}\n" +
            $"• 平行光颜色: {directionalLight.color}\n" +
            $"• 4套光照配置（BaseScene/Chunk × 预览/正式）\n\n" +
            "后续烘焙时会自动应用对应配置。",
            "确定");

        Debug.Log($"[光照配置] 已{message}全局光照Prefab: {prefabPath}");

        return true;
    }

    /// <summary>
    /// 应用全局光照配置到当前场景（临时应用，烘焙后应删除）
    /// </summary>
    /// <param name="globalLightingPrefab">全局光照配置Prefab</param>
    /// <param name="isFinal">是否为正式烘焙（false=预览）</param>
    /// <param name="isBaseScene">是否为BaseScene（false=Chunk）</param>
    public static bool ApplyGlobalLighting(GameObject globalLightingPrefab, bool isFinal, bool isBaseScene)
    {
        if (globalLightingPrefab == null)
        {
            Debug.LogWarning("[光照配置] 全局光照配置为空，跳过应用");
            return false;
        }

        Scene currentScene = EditorSceneManager.GetActiveScene();

        // 删除场景中现有的 GlobalLighting 实例和平行光
        RemoveGlobalLightingFromScene();

        // 实例化全局光照配置（临时用于烘焙）
        GameObject lightingInstance = (GameObject)PrefabUtility.InstantiatePrefab(globalLightingPrefab);
        SceneManager.MoveGameObjectToScene(lightingInstance, currentScene);
    
        // 重命名，标记为临时的
        lightingInstance.name = "GlobalLighting_Temp";

        // 应用光照贴图设置
        LightmapSettingsStorage storage = lightingInstance.GetComponentInChildren<LightmapSettingsStorage>();
        if (storage != null)
        {
            storage.ApplySettings(isFinal, isBaseScene);
        }
        else
        {
            Debug.LogWarning("[光照配置] 未找到LightmapSettingsStorage组件");
        }

        string modeStr = isFinal ? "正式" : "预览";
        string sceneStr = isBaseScene ? "BaseScene" : "Chunk";
        Debug.Log($"[光照配置] 已应用 {sceneStr} {modeStr} 光照配置到场景: {currentScene.name}（临时，烘焙后会自动删除）");
        return true;
    }
    
    /// <summary>
    /// 从场景中移除全局光照配置和所有平行光
    /// </summary>
    public static void RemoveGlobalLightingFromScene()
    {
        // 删除 GlobalLighting 实例（包括 _Temp 后缀的）
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            if (obj.name.StartsWith("GlobalLighting"))
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}