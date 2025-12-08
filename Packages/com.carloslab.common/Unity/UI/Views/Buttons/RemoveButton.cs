// <copyright file="RemoveButton.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CarlosLab.Common.UI
{
    [UxmlElement]
    public partial class RemoveButton : Button
    {
        private bool styleSheetsLoaded;

        public void UpdateView(bool isRuntimeUI)
        {
            LoadStyleSheets(isRuntimeUI);
        }

        private void LoadStyleSheets(bool isRuntimeUI)
        {
            if (styleSheetsLoaded) return;

#if UNITY_EDITOR

            if (isRuntimeUI)
                LoadStyleSheet(UIBuilderResourcePaths.RemoveButton_Light_Runtime);
            else if (EditorGUIUtility.isProSkin)
                LoadStyleSheet(UIBuilderResourcePaths.RemoveButton_Dark);
            else
                LoadStyleSheet(UIBuilderResourcePaths.RemoveButton_Light);
#else
            LoadStyleSheet(UIBuilderResourcePaths.RemoveButton_Light_Runtime);
#endif

            AddToClassList("remove-button");

            styleSheetsLoaded = true;
        }

        protected void LoadStyleSheet(string styleSheetPath)
        {
            if (string.IsNullOrEmpty(styleSheetPath))
                return;

            StyleSheet styleSheet = Resources.Load<StyleSheet>(styleSheetPath);
            if (styleSheet == null) return;

            styleSheets.Add(styleSheet);
        }
    }
}