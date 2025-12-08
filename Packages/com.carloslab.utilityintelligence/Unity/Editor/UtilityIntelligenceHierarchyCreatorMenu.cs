// <copyright file="UtilityIntelligenceHierarchyCreatorMenu.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEditor;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class UtilityIntelligenceHierarchyCreatorMenu
    {
        [MenuItem(FrameworkEditorConsts.CreateUtilityWorldMenuPath, false, 0)]
        private static void CreateUtilityWorld(MenuCommand menuCommand)
        {
            string prefabPath = FrameworkEditorConsts.UtilityWorldPrefabPath;
            SpawnPrefab(prefabPath, Selection.activeTransform);
        }

        [MenuItem(FrameworkEditorConsts.CreateRuntimeEditorMenuPath, false, 0)]
        private static void CreateRuntimeView(MenuCommand menuCommand)
        {
            string prefabPath = FrameworkEditorConsts.RuntimeEditorPrefabPath;
            SpawnPrefab(prefabPath, Selection.activeTransform);
        }

        private static void SpawnPrefab(string prefabPath, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at path: {prefabPath}");
                return;
            }

            GameObject instance = Object.Instantiate(prefab, parent, false);
            instance.name = prefab.name;

            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeGameObject = instance;
        }
    }
}