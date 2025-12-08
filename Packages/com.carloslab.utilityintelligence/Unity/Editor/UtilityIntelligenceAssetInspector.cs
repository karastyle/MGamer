// <copyright file="UtilityIntelligenceAssetInspector.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEditor;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.Editor
{
    [CustomEditor(typeof(UtilityIntelligenceAsset))]
    public class UtilityIntelligenceAssetInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new();
            inspector.dataSource = serializedObject.targetObject;

            TextField typeField = new("Type");
            typeField.textEdition.placeholder = "Type...";
            typeField.bindingPath = nameof(UtilityIntelligenceAsset.type);
            inspector.Add(typeField);

            TextField descriptionField = new("Description");
            descriptionField.textEdition.placeholder = "Description...";
            descriptionField.bindingPath = nameof(UtilityIntelligenceAsset.description);
            inspector.Add(descriptionField);

            IntegerField dataVersionField = new("Data Version");
            dataVersionField.bindingPath = nameof(UtilityIntelligenceAsset.dataVersion);
            dataVersionField.SetEnabled(false);
            inspector.Add(dataVersionField);

            Button openEditorButton = new();
            openEditorButton.text = "Open Editor";
            openEditorButton.clicked += () =>
            {
                if (serializedObject.targetObject is UtilityIntelligenceAsset asset)
                    UtilityIntelligenceEditor.OpenWindow(asset);
            };

            inspector.Add(openEditorButton);

            return inspector;
        }
    }
}