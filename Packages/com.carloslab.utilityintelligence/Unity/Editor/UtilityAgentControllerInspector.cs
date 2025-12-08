// <copyright file="UtilityAgentControllerInspector.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.Editor
{
    [CustomEditor(typeof(UtilityAgentController))]
    public class UtilityAgentControllerInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new();

            ObjectField assetField = new();
            assetField.label = "Intelligence Asset";
            assetField.bindingPath = nameof(UtilityAgentController.editorAsset);

            inspector.Add(assetField);

            Button openEditorButton = new();
            openEditorButton.text = "Open Editor";
            openEditorButton.clicked += () =>
            {
                UtilityAgentController controller = (UtilityAgentController)target;

                var asset = controller.EditorAsset;
                if (asset == null)
                {
                    Debug.LogError($"{controller.Name} contains null IntelligenceAsset");
                    return;
                }

                UtilityIntelligenceEditor.OpenWindow(asset);
            };

            inspector.Add(openEditorButton);

            return inspector;
        }
    }
}