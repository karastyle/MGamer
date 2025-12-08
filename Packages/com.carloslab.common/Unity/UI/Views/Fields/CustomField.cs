// <copyright file="CustomField.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public class CustomFieldInput : VisualElement
    {
        public const string VisualInputName = "VisualInput";

        public CustomFieldInput()
        {
            name = VisualInputName;
            pickingMode = PickingMode.Ignore;
            style.flexDirection = FlexDirection.Row;
        }
    }

    public class CustomField<TValueType> : BaseField<TValueType>
    {
        private VisualTreeAsset visualAsset;

        public CustomField(string label, string visualAssetPath) : this(label, new CustomFieldInput(), visualAssetPath)
        {
        }
        public CustomField(string label, VisualElement input, string visualAssetPath) : base(label, input)
        {
            VisualInput = this.Q<VisualElement>(CustomFieldInput.VisualInputName);

            // Debug.Log($"{GetType().Name} Constructor Label: {label} ChildCount: {childCount}");
            //style.flexGrow = 1.0f;

            LoadVisualAsset(visualAssetPath);

            RegisterCallback<AttachToPanelEvent>(HandleAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(HandleDetachFromPanel);
        }

        protected void LoadVisualAsset(string visualAssetPath)
        {
            if (string.IsNullOrEmpty(visualAssetPath))
                return;

            visualAsset = Resources.Load<VisualTreeAsset>(visualAssetPath);
            if (visualAsset == null) return;

            visualAsset.CloneTree(VisualInput);

            OnVisualAssetLoaded();
        }

        protected virtual void OnVisualAssetLoaded()
        {

        }

        public VisualElement VisualInput { get; }

        private void HandleAttachToPanel(AttachToPanelEvent evt)
        {
            OnAttachToPanel(evt);
        }

        private void HandleDetachFromPanel(DetachFromPanelEvent evt)
        {
            OnDetachFromPanel(evt);
        }

        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
        }

        protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
        }

        protected void AddChild(VisualElement child)
        {
            // Debug.Log($"{GetType().Name} Label: {label} AddChild: {child.GetType().Name}  ChildCount: {childCount}");
            VisualInput.Add(child);
        }

        protected void RemoveChild(VisualElement child)
        {
            VisualInput.Remove(child);
        }

        protected void InsertChild(int index, VisualElement child)
        {
            VisualInput.Insert(index, child);
        }

        protected void RemoveAllChildren()
        {
            VisualInput.Clear();
        }
    }
}