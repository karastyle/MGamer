#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(LightmapSettingsStorage))]
public class LightmapSettingsStorageEditor : Editor
{
    SerializedProperty settingsProp;
    SerializedProperty baseScenePreview;
    SerializedProperty baseSceneFinal;
    SerializedProperty chunkPreview;
    SerializedProperty chunkFinal;
    
    private void OnEnable()
    {
        settingsProp = serializedObject.FindProperty("settings");
        baseScenePreview = serializedObject.FindProperty("baseScenePreview");
        baseSceneFinal = serializedObject.FindProperty("baseSceneFinal");
        chunkPreview = serializedObject.FindProperty("chunkPreview");
        chunkFinal = serializedObject.FindProperty("chunkFinal");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LightmapSettingsStorage storage = (LightmapSettingsStorage)target;
        
        // Settings 展开
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("skyboxMaterial"));
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("sunSource"));
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("realtimeShadowColor"));
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Environment Lighting", EditorStyles.boldLabel);
        
        // Ambient Mode
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("ambientMode"));
        
        // 根据 ambientMode 显示对应的参数
        var ambientMode = (AmbientMode)settingsProp.FindPropertyRelative("ambientMode").enumValueFlag;
        
        switch (ambientMode)
        {
            case AmbientMode.Skybox:
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("ambientIntensity"), new GUIContent("Intensity Multiplier"));
                break;
            case AmbientMode.Trilight:
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("ambientSkyColor"), new GUIContent("Sky Color"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("ambientEquatorColor"), new GUIContent("Equator Color"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("ambientGroundColor"), new GUIContent("Ground Color"));
                break;
            case AmbientMode.Flat:
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("ambientLight"), new GUIContent("Ambient Color"));
                break;
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Environment Reflections", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("defaultReflectionMode"), new GUIContent("Source"));
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("defaultReflectionResolution"), new GUIContent("Resolution"));
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("reflectionCompression"), new GUIContent("Compression"));
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("reflectionIntensity"), new GUIContent("Intensity Multiplier"));
        EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("reflectionBounces"), new GUIContent("Bounces"));
        
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(baseScenePreview);
        EditorGUILayout.PropertyField(baseSceneFinal);
        EditorGUILayout.PropertyField(chunkPreview);
        EditorGUILayout.PropertyField(chunkFinal);
        
        
        EditorGUILayout.Space(10);
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif