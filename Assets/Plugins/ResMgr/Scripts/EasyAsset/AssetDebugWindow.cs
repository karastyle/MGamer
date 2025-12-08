// AssetDebugWindow.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using EasyTools;

public class AssetDebugWindow : EditorWindow
{
    [MenuItem("Tools/Asset Debug Window")]
    private static void OpenWindow()
    {
        var window = GetWindow<AssetDebugWindow>("Asset Debug");
        window.minSize = new Vector2(1000, 600);
        window.Show();
    }

    private bool isDebugging = false;
    private BundleLoader selectedBundle;
    private ProviderBase selectedProvider;
    
    private Vector2 scrollPos1, scrollPos2, scrollPos3, scrollPos4, scrollPos5;
    private GUIStyle headerStyle;
    private GUIStyle rowStyle;
    private GUIStyle selectedRowStyle;

    private void OnEnable()
    {
        InitStyles();
    }

    private void InitStyles()
    {
        headerStyle = new GUIStyle();
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.padding = new RectOffset(5, 5, 5, 5);

        rowStyle = new GUIStyle();
        rowStyle.normal.textColor = Color.white;
        rowStyle.alignment = TextAnchor.MiddleLeft;
        rowStyle.padding = new RectOffset(5, 5, 3, 3);

        selectedRowStyle = new GUIStyle(rowStyle);
        selectedRowStyle.normal.background = MakeTexture(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.5f));
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void OnGUI()
    {
        if (headerStyle == null) InitStyles();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("需要在编辑器下运行游戏才能使用此工具", MessageType.Warning);
            isDebugging = false;
            return;
        }

        DrawToolbar();

        if (!isDebugging) return;

        if (EasyAsset.Instance == null)
        {
            EditorGUILayout.HelpBox("EasyAsset未初始化", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        DrawBundleList();
        EditorGUILayout.Space(5);
        DrawDependencies();
        EditorGUILayout.Space(5);
        DrawReferencedBy();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        DrawProviderList();

        EditorGUILayout.Space(5);
        DrawHandleList();

        if (GUI.changed) Repaint();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (!isDebugging)
        {
            if (GUILayout.Button("开始调试", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                isDebugging = true;
                selectedBundle = null;
                selectedProvider = null;
            }
        }
        else
        {
            if (GUILayout.Button("停止调试", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                isDebugging = false;
                selectedBundle = null;
                selectedProvider = null;
            }

            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Repaint();
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawBundleList()
    {
        float width = (position.width - 20) / 3;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width), GUILayout.Height(200));

        EditorGUILayout.LabelField("Bundles (RefCount > 0)", EditorStyles.boldLabel);

        DrawTableHeader(new string[] { "Bundle名称", "加载时间", "引用数" }, 
                       new float[] { 0.6f, 0.2f, 0.2f }, width - 20);

        scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, GUILayout.Height(160));

        var loaders = EasyAsset.Instance.GetLoadedBundlesWithRef();
        foreach (var loader in loaders)
        {
            bool isSelected = selectedBundle == loader;
            DrawBundleRow(loader, isSelected, width - 20, () =>
            {
                selectedBundle = loader;
                selectedProvider = null;
            });
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDependencies()
    {
        float width = (position.width - 20) / 3;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width), GUILayout.Height(200));

        EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);

        DrawTableHeader(new string[] { "Bundle名称", "加载时间", "引用数" }, 
                       new float[] { 0.6f, 0.2f, 0.2f }, width - 20);

        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.Height(160));

        if (selectedBundle != null)
        {
            foreach (var dep in selectedBundle.DependLoaders)
            {
                DrawBundleRow(dep, false, width - 20, null);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawReferencedBy()
    {
        float width = (position.width - 20) / 3;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width), GUILayout.Height(200));

        EditorGUILayout.LabelField("Referenced By", EditorStyles.boldLabel);

        DrawTableHeader(new string[] { "Bundle名称", "加载时间", "引用数" }, 
                       new float[] { 0.6f, 0.2f, 0.2f }, width - 20);

        scrollPos3 = EditorGUILayout.BeginScrollView(scrollPos3, GUILayout.Height(160));

        if (selectedBundle != null)
        {
            var refBy = selectedBundle.GetReferencedBy(EasyAsset.Instance.GetAllBundleLoaders());
            foreach (var loader in refBy)
            {
                DrawBundleRow(loader, false, width - 20, null);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawProviderList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(150));

        EditorGUILayout.LabelField("AssetProviders Using This Bundle", EditorStyles.boldLabel);

        float width = position.width - 30;
        DrawTableHeader(new string[] { "资源类型", "资源名称", "加载时间", "引用数" }, 
                       new float[] { 0.15f, 0.55f, 0.15f, 0.15f }, width);

        scrollPos4 = EditorGUILayout.BeginScrollView(scrollPos4, GUILayout.Height(110));

        if (selectedBundle != null)
        {
            var providers = EasyAsset.Instance.GetProvidersUsingBundle(selectedBundle);
            foreach (var provider in providers)
            {
                bool isSelected = selectedProvider == provider;
                DrawProviderRow(provider, isSelected, width, () =>
                {
                    selectedProvider = provider;
                });
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawHandleList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(150));

        EditorGUILayout.LabelField("AssetHandles Using This Provider", EditorStyles.boldLabel);

        float width = position.width - 30;
        DrawTableHeader(new string[] { "Handle类型", "创建时间", "引用数" }, 
                       new float[] { 0.4f, 0.3f, 0.3f }, width);

        scrollPos5 = EditorGUILayout.BeginScrollView(scrollPos5, GUILayout.Height(110));

        if (selectedProvider != null)
        {
            foreach (var handle in selectedProvider.Handles)
            {
                DrawHandleRow(handle, width);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawTableHeader(string[] headers, float[] widthRatios, float totalWidth)
    {
        EditorGUILayout.BeginHorizontal();
        
        for (int i = 0; i < headers.Length; i++)
        {
            EditorGUILayout.LabelField(headers[i], headerStyle, GUILayout.Width(totalWidth * widthRatios[i]));
        }
        
        EditorGUILayout.EndHorizontal();
        
        Rect rect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height, rect.width, 1), new Color(0.5f, 0.5f, 0.5f));
    }

    private void DrawBundleRow(BundleLoader loader, bool isSelected, float totalWidth, Action onClick)
    {
        EditorGUILayout.BeginHorizontal(isSelected ? selectedRowStyle : rowStyle);

        EditorGUILayout.LabelField(loader.BundleName, GUILayout.Width(totalWidth * 0.6f));
        EditorGUILayout.LabelField(loader.LoadCompleteTime.ToString("HH:mm"), GUILayout.Width(totalWidth * 0.2f));
        EditorGUILayout.LabelField(loader.RefCount.ToString(), GUILayout.Width(totalWidth * 0.2f));

        EditorGUILayout.EndHorizontal();

        Rect rect = GUILayoutUtility.GetLastRect();
        if (onClick != null && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            onClick();
            Event.current.Use();
            Repaint();
        }
    }

    private void DrawProviderRow(ProviderBase provider, bool isSelected, float totalWidth, Action onClick)
    {
        EditorGUILayout.BeginHorizontal(isSelected ? selectedRowStyle : rowStyle);

        EditorGUILayout.LabelField(provider.GetAssetTypeName(), GUILayout.Width(totalWidth * 0.15f));
        EditorGUILayout.LabelField(System.IO.Path.GetFileName(provider.Location), GUILayout.Width(totalWidth * 0.55f));
        EditorGUILayout.LabelField(provider.LoadCompleteTime.ToString("HH:mm"), GUILayout.Width(totalWidth * 0.15f));
        EditorGUILayout.LabelField(provider.RefCount.ToString(), GUILayout.Width(totalWidth * 0.15f));

        EditorGUILayout.EndHorizontal();

        Rect rect = GUILayoutUtility.GetLastRect();
        if (onClick != null && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            onClick();
            Event.current.Use();
            Repaint();
        }
    }

    private void DrawHandleRow(HandleBase handle, float totalWidth)
    {
        EditorGUILayout.BeginHorizontal(rowStyle);

        EditorGUILayout.LabelField(handle.GetType().Name, GUILayout.Width(totalWidth * 0.4f));
        EditorGUILayout.LabelField(handle.CreateTime.ToString("HH:mm"), GUILayout.Width(totalWidth * 0.3f));
        EditorGUILayout.LabelField(handle.HandleRefCount.ToString(), GUILayout.Width(totalWidth * 0.3f));

        EditorGUILayout.EndHorizontal();
    }

    private void Update()
    {
        if (isDebugging && Application.isPlaying)
        {
            Repaint();
        }
    }

    private void OnDestroy()
    {
        isDebugging = false;
    }
}
#endif