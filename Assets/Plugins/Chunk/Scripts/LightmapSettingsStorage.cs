using System;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 存储和应用光照烘焙相关设置
/// 只包含影响烘焙结果的参数
/// </summary>
public class LightmapSettingsStorage : MonoBehaviour
{
    [System.Serializable]
    public class Settings
    {
        // Environment - Skybox Material
        public Material skyboxMaterial;
        
        // Environment - Sun Source
        public Light sunSource;
        
        // Environment - Realtime Shadow Color
        public Color realtimeShadowColor = Color.black;
        
        // Environment Lighting - Source
        public AmbientMode ambientMode = AmbientMode.Skybox;
        
        // Environment Lighting - Intensity Multiplier (for Skybox mode)
        [HideInInspector]
        public float ambientIntensity = 1.0f;
        
        // Environment Lighting - Ambient Color (for Gradient mode)
        [HideInInspector]
        public Color ambientSkyColor = new Color(0.212f, 0.227f, 0.259f);
        [HideInInspector]
        public Color ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f);
        [HideInInspector]
        public Color ambientGroundColor = new Color(0.047f, 0.043f, 0.035f);
        
        // Environment Lighting - Ambient Color (for Color mode)
        [HideInInspector]
        public Color ambientLight = new Color(0.212f, 0.227f, 0.259f);
        
        // Environment Reflections - Source
        public DefaultReflectionMode defaultReflectionMode = DefaultReflectionMode.Skybox;
        
        // Environment Reflections - Resolution
        public int defaultReflectionResolution = 128;
        
        // Environment Reflections - Compression
        public ReflectionCubemapCompression reflectionCompression = ReflectionCubemapCompression.Auto;
        
        // Environment Reflections - Intensity Multiplier
        public float reflectionIntensity = 1.0f;
        
        // Environment Reflections - Bounces
        public int reflectionBounces = 1;
    }
    
    public Settings settings = new Settings();
    
    [Header("Lighting Settings Assets")]
    [Tooltip("BaseScene预览模式的Lighting Settings")]
    public LightingSettings baseScenePreview;
    
    [Tooltip("BaseScene正式模式的Lighting Settings")]
    public LightingSettings baseSceneFinal;
    
    [Tooltip("Chunk预览模式的Lighting Settings")]
    public LightingSettings chunkPreview;
    
    [Tooltip("Chunk正式模式的Lighting Settings")]
    public LightingSettings chunkFinal;
    
    /// <summary>
    /// 应用保存的光照烘焙设置到当前场景
    /// </summary>
    /// <param name="isFinal">是否为正式烘焙（false=预览）</param>
    /// <param name="isBaseScene">是否为BaseScene（false=Chunk）</param>
    public void ApplySettings(bool isFinal, bool isBaseScene)
    {
        // Environment
        RenderSettings.skybox = settings.skyboxMaterial;
        RenderSettings.sun = settings.sunSource;
        RenderSettings.subtractiveShadowColor = settings.realtimeShadowColor;
        
        // Environment Lighting - 先设置模式
        RenderSettings.ambientMode = settings.ambientMode;
        
        // 根据不同模式应用对应的参数
        switch (settings.ambientMode)
        {
            case AmbientMode.Skybox:
                RenderSettings.ambientIntensity = settings.ambientIntensity;
                break;
            case AmbientMode.Trilight:
                RenderSettings.ambientSkyColor = settings.ambientSkyColor;
                RenderSettings.ambientEquatorColor = settings.ambientEquatorColor;
                RenderSettings.ambientGroundColor = settings.ambientGroundColor;
                break;
            case AmbientMode.Flat:
                RenderSettings.ambientLight = settings.ambientLight;
                break;
        }
        
        // Environment Reflections
        RenderSettings.defaultReflectionMode = settings.defaultReflectionMode;
        RenderSettings.defaultReflectionResolution = settings.defaultReflectionResolution;
        RenderSettings.reflectionIntensity = settings.reflectionIntensity;
        RenderSettings.reflectionBounces = settings.reflectionBounces;
        
        // 应用对应的 Lighting Settings Asset
        #if UNITY_EDITOR
        LightingSettings targetSettings = GetLightingSettings(isFinal, isBaseScene);
        
        if (targetSettings != null)
        {
            Lightmapping.lightingSettings = targetSettings;
            
            string modeStr = isFinal ? "正式" : "预览";
            string sceneStr = isBaseScene ? "BaseScene" : "Chunk";
            Debug.Log($"[光照配置] 已应用 {sceneStr} {modeStr} Lighting Settings: {targetSettings.name}");
        }
        else
        {
            string modeStr = isFinal ? "正式" : "预览";
            string sceneStr = isBaseScene ? "BaseScene" : "Chunk";
            Debug.LogWarning($"[光照配置] {sceneStr} {modeStr} Lighting Settings 未设置");
        }
        #endif
        
        Debug.Log("[光照配置] 已应用完整光照设置");
    }
    
    /// <summary>
    /// 获取对应的Lighting Settings
    /// </summary>
    private LightingSettings GetLightingSettings(bool isFinal, bool isBaseScene)
    {
        if (isBaseScene)
        {
            return isFinal ? baseSceneFinal : baseScenePreview;
        }
        else
        {
            return isFinal ? chunkFinal : chunkPreview;
        }
    }
}