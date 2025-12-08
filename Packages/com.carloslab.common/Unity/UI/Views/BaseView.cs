// <copyright file="BaseView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Runtime.CompilerServices;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CarlosLab.Common.UI
{
    public abstract class BaseView : VisualElement, IView
    {
        public string Id { get; }

        private VisualTreeAsset visualAsset;

        protected bool hasLoadedVisualAsset;

        public BaseView(string visualAssetPath)
        {
            Id = Guid.NewGuid().ToString();

            LoadVisualAsset(visualAssetPath);

            RegisterCallback<AttachToPanelEvent>(AttachToPanelEventHandler);
            RegisterCallback<DetachFromPanelEvent>(DetachFromPanelEventHandler);

            RegisterCallback<GeometryChangedEvent>(GeometryChangedEventHandler);
        }

        public event Action Shown;
        public event Action Hidden;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        #region Visual/Style Functions

        protected void LoadVisualAsset(string visualAssetPath)
        {
            if (hasLoadedVisualAsset) return;

            if (string.IsNullOrEmpty(visualAssetPath))
            {
                OnLoadVisualAssetFailed();
                return;
            }

            visualAsset = Resources.Load<VisualTreeAsset>(visualAssetPath);

            if (visualAsset != null)
            {
                visualAsset.CloneTree(this);

                hasLoadedVisualAsset = true;
                OnLoadVisualAssetSuccess();
            }
            else
            {
                OnLoadVisualAssetFailed();
            }
        }

        protected virtual void OnLoadVisualAssetSuccess()
        {

        }

        protected virtual void OnLoadVisualAssetFailed()
        {

        }

        protected void LoadStyleSheet(string styleSheetPath)
        {
            if (string.IsNullOrEmpty(styleSheetPath))
                return;

            StyleSheet styleSheet = Resources.Load<StyleSheet>(styleSheetPath);
            if (styleSheet == null) return;

            styleSheets.Add(styleSheet);
        }

        #endregion

        #region Show/Hide View Functions

        public void Show(bool show)
        {
            this.SetDisplay(show);
            if (show)
                HandleShownEvent();
            else
                HandleHiddenEvent();
        }

        public void HandleShownEvent()
        {
            Shown?.Invoke();
            OnShown();
        }

        public void HandleHiddenEvent()
        {
            Hidden?.Invoke();
            OnHidden();
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }

        #endregion

        #region Attach/Detach Panel Functions

        private void AttachToPanelEventHandler(AttachToPanelEvent evt)
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
            OnAttachToPanel(evt);
        }

        private void DetachFromPanelEventHandler(DetachFromPanelEvent evt)
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
            OnDetachFromPanel(evt);
        }

        private void GeometryChangedEventHandler(GeometryChangedEvent evt)
        {
            OnGeometryChanged(evt);
        }

#if UNITY_EDITOR
        protected virtual void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnEnterEditMode();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnEnterPlayerMode();
                    break;
            }
        }

        protected virtual void OnEnterEditMode()
        {
        }

        protected virtual void OnEnterPlayerMode()
        {
        }
#endif

        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
        }

        protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
        }

        protected virtual void OnGeometryChanged(GeometryChangedEvent evt)
        {
        }

        #endregion
    }
}