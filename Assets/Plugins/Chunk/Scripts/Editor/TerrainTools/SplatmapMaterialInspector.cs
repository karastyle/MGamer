using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimpleTerrainToMesh.Editor
{
    /// <summary>
    /// Splatmap材质的完整自定义Inspector
    /// 支持可视化编辑和实时预览
    /// 支持 Texture 2D Array 模式
    /// </summary>
    public class SplatmapMaterialInspector : ShaderGUI
    {
        // UI状态
        private int selectedLayerIndex = -1;
        private bool showSplatmaps = false;
        private bool showKeywords = false;
        private Vector2 scrollPosition;

        // 材质属性缓存
        private MaterialProperty[] properties;
        private MaterialEditor materialEditor;

        // 缓存从Texture2DArray生成的预览纹理
        private Dictionary<string, Texture2D> arrayPreviewCache = new Dictionary<string, Texture2D>();

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            this.materialEditor = materialEditor;
            this.properties = properties;
            Material material = materialEditor.target as Material;

            EditorGUILayout.Space(5);

            // 检测是否使用Texture2DArray并显示提示
            bool usingSplatmapsArray = material.GetTexture("_T2M_SplatMaps2DArray") != null;
            bool usingDiffuseArray = material.GetTexture("_T2M_DiffuseMaps2DArray") != null;
            bool usingNormalArray = material.GetTexture("_T2M_NormalMaps2DArray") != null;
            bool usingMaskArray = material.GetTexture("_T2M_MaskMaps2DArray") != null;
            bool usingTextureArrays = usingSplatmapsArray || usingDiffuseArray || usingNormalArray || usingMaskArray;

            if (usingTextureArrays)
            {
                EditorGUILayout.HelpBox("此材质使用 Texture 2D Array 模式\n纹理由数组统一管理，不支持单独编辑", MessageType.Info);
                EditorGUILayout.Space(3);
            }

            // 滚动区域
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Splatmaps区域
            DrawSplatmapsSection(material, usingSplatmapsArray);

            EditorGUILayout.Space(5);

            // Layers区域
            DrawLayersSection(material, usingDiffuseArray, usingNormalArray, usingMaskArray);

            EditorGUILayout.Space(5);

            // 选中Layer的详细信息
            if (selectedLayerIndex >= 0)
            {
                DrawSelectedLayerDetails(material, selectedLayerIndex, usingDiffuseArray, usingNormalArray, usingMaskArray);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 从Texture2DArray的指定层生成预览纹理
        /// </summary>
        private Texture2D GenerateArrayLayerPreview(Texture2DArray textureArray, int layer, int previewSize = 64)
        {
            if (textureArray == null || layer < 0 || layer >= textureArray.depth)
                return null;

            string cacheKey = $"{textureArray.GetInstanceID()}_{layer}_{previewSize}";

            // 检查缓存
            if (arrayPreviewCache.TryGetValue(cacheKey, out Texture2D cached))
            {
                if (cached != null) return cached;
            }

            try
            {
                // 步骤1: 创建与源纹理相同大小的临时RT
                RenderTexture tempSource = RenderTexture.GetTemporary(textureArray.width,
                    textureArray.height,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.sRGB);

                // 步骤2: 复制数组的指定层到临时RT
                Graphics.CopyTexture(textureArray, layer, 0, tempSource, 0, 0);

                // 步骤3: 创建预览大小的RT并缩放
                RenderTexture tempPreview = RenderTexture.GetTemporary(previewSize,
                    previewSize,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.sRGB);
                tempPreview.filterMode = FilterMode.Bilinear;

                // 使用Blit缩放
                Graphics.Blit(tempSource, tempPreview);

                // 步骤4: 读取到Texture2D
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tempPreview;

                Texture2D preview = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false, true);
                preview.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
                preview.Apply();

                RenderTexture.active = previous;

                // 清理
                RenderTexture.ReleaseTemporary(tempSource);
                RenderTexture.ReleaseTemporary(tempPreview);

                // 缓存
                arrayPreviewCache[cacheKey] = preview;

                return preview;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to generate preview for layer {layer}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 绘制Splatmaps区域
        /// </summary>
        private void DrawSplatmapsSection(Material material, bool usingSplatmapsArray)
        {
            showSplatmaps = EditorGUILayout.Foldout(showSplatmaps, "Splatmaps", true, EditorStyles.foldoutHeader);

            if (showSplatmaps)
            {
                if (usingSplatmapsArray)
                {
                    // 显示Texture2DArray每一层的预览
                    Texture2DArray splatmapsArray = material.GetTexture("_T2M_SplatMaps2DArray") as Texture2DArray;
                    if (splatmapsArray != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("Splatmaps Array", EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Resolution: {splatmapsArray.width} x {splatmapsArray.height}", EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Depth: {splatmapsArray.depth} layers", EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();

                        // 显示每一层的缩略图
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Layers:", EditorStyles.miniLabel);
                        EditorGUILayout.BeginHorizontal();

                        int layerCount = Mathf.Min(4, splatmapsArray.depth);
                        for (int i = 0; i < layerCount; i++)
                        {
                            DrawArrayLayerThumbnail(splatmapsArray, i, 64);
                        }

                        EditorGUILayout.EndHorizontal();

                        if (GUILayout.Button("Locate Asset", GUILayout.Width(128)))
                        {
                            EditorGUIUtility.PingObject(splatmapsArray);
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    // 显示单独的Splatmap纹理
                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < 4; i++)
                    {
                        DrawSplatmapThumbnail(material, i, false);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        /// <summary>
        /// 绘制数组层的缩略图
        /// </summary>
        private void DrawArrayLayerThumbnail(Texture2DArray textureArray, int layer, int size)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(size + 10));

            EditorGUILayout.LabelField($"[{layer}]", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(size + 10));

            Rect thumbnailRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));

            EditorGUI.DrawRect(thumbnailRect, new Color(0.2f, 0.2f, 0.2f));

            Texture2D preview = GenerateArrayLayerPreview(textureArray, layer, size);
            if (preview != null)
            {
                EditorGUI.DrawPreviewTexture(thumbnailRect, preview);
            }
            else
            {
                EditorGUI.LabelField(thumbnailRect, "N/A", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单个Splatmap缩略图
        /// </summary>
        private void DrawSplatmapThumbnail(Material material, int index, bool isDisabled)
        {
            string propertyName = $"_T2M_SplatMap_{index}";
            Texture splatmap = material.GetTexture(propertyName);

            EditorGUILayout.BeginVertical(GUILayout.Width(80));

            // 缩略图容器
            Rect containerRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));

            // 绘制背景
            Color bgColor = isDisabled ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.2f, 0.2f, 0.2f);
            EditorGUI.DrawRect(containerRect, bgColor);

            if (splatmap != null)
            {
                Color oldColor = GUI.color;
                if (isDisabled) GUI.color = new Color(1, 1, 1, 0.5f);

                EditorGUI.DrawPreviewTexture(containerRect, splatmap);

                if (isDisabled) GUI.color = oldColor;
            }

            // Select按钮（右下角）
            Rect selectRect = new Rect(containerRect.x + containerRect.width - 55, containerRect.y + containerRect.height - 18, 50, 16);
            EditorGUI.BeginDisabledGroup(isDisabled);
            if (GUI.Button(selectRect, "Select", EditorStyles.miniButton))
            {
                if (!isDisabled)
                {
                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
                    EditorGUIUtility.ShowObjectPicker<Texture2D>(splatmap, false, "", controlID);
                    GUIUtility.keyboardControl = controlID;
                    EditorPrefs.SetString("SplatmapPicker_Property", propertyName);
                }
            }

            EditorGUI.EndDisabledGroup();

            if (!isDisabled)
            {
                if (Event.current.type == EventType.MouseDown && containerRect.Contains(Event.current.mousePosition) &&
                    !selectRect.Contains(Event.current.mousePosition))
                {
                    if (splatmap != null)
                    {
                        EditorGUIUtility.PingObject(splatmap);
                    }

                    Event.current.Use();
                }

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete &&
                    containerRect.Contains(Event.current.mousePosition))
                {
                    if (splatmap != null)
                    {
                        Undo.RecordObject(material, "Clear Splatmap");
                        material.SetTexture(propertyName, null);
                        EditorUtility.SetDirty(material);
                        Event.current.Use();
                    }
                }
            }

            EditorGUILayout.EndVertical();

            if (!isDisabled && (Event.current.commandName == "ObjectSelectorUpdated" || Event.current.commandName == "ObjectSelectorClosed"))
            {
                string pickerProperty = EditorPrefs.GetString("SplatmapPicker_Property", "");
                if (pickerProperty == propertyName)
                {
                    Texture newTexture = EditorGUIUtility.GetObjectPickerObject() as Texture;
                    if (newTexture != splatmap)
                    {
                        Undo.RecordObject(material, "Change Splatmap");
                        material.SetTexture(propertyName, newTexture);
                        EditorUtility.SetDirty(material);
                        GUI.changed = true;
                    }

                    if (Event.current.commandName == "ObjectSelectorClosed")
                    {
                        EditorPrefs.DeleteKey("SplatmapPicker_Property");
                    }
                }
            }
        }

        /// <summary>
        /// 绘制Layers区域
        /// </summary>
        private void DrawLayersSection(Material material, bool usingDiffuseArray, bool usingNormalArray, bool usingMaskArray)
        {
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);

            float layerCount = material.GetFloat("_T2M_Layer_Count");
            if (layerCount <= 0) return;

            if (usingDiffuseArray)
            {
                EditorGUILayout.HelpBox("图层纹理使用 Texture 2D Array，预览从数组读取", MessageType.Info);
            }

            int columns = 5;
            int rows = Mathf.CeilToInt(layerCount / (float)columns);
            float thumbnailSize = 64;

            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int col = 0; col < columns; col++)
                {
                    int layerIndex = row * columns + col;
                    if (layerIndex >= (int)layerCount) break;

                    DrawLayerThumbnail(material, layerIndex, thumbnailSize, usingDiffuseArray);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制单个Layer缩略图
        /// </summary>
        private void DrawLayerThumbnail(Material material, int layerIndex, float size, bool usingDiffuseArray)
        {
            string prefix = $"_T2M_Layer_{layerIndex}";
            Texture2D previewTexture = null;

            // 根据是否使用数组获取预览
            if (usingDiffuseArray)
            {
                Texture2DArray diffuseArray = material.GetTexture("_T2M_DiffuseMaps2DArray") as Texture2DArray;
                if (diffuseArray != null)
                {
                    previewTexture = GenerateArrayLayerPreview(diffuseArray, layerIndex, (int)size);
                }
            }
            else
            {
                Texture diffuse = material.GetTexture($"{prefix}_Diffuse");
                previewTexture = diffuse as Texture2D;
            }

            bool isSelected = (selectedLayerIndex == layerIndex);
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            if (isSelected)
            {
                boxStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.5f));
            }

            EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(size + 4), GUILayout.Height(size + 4));

            Rect thumbnailRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));

            EditorGUI.DrawRect(thumbnailRect, new Color(0.3f, 0.3f, 0.3f));

            if (previewTexture != null)
            {
                EditorGUI.DrawPreviewTexture(thumbnailRect, previewTexture);
            }

            // 显示层索引标签
            GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
            labelStyle.alignment = TextAnchor.LowerRight;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = new Color(1, 1, 1, 0.8f);
            GUI.Label(thumbnailRect, $" {layerIndex} ", labelStyle);

            if (Event.current.type == EventType.MouseDown && thumbnailRect.Contains(Event.current.mousePosition))
            {
                selectedLayerIndex = (selectedLayerIndex == layerIndex) ? -1 : layerIndex;
                Event.current.Use();
                GUI.changed = true;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制选中Layer的详细信息
        /// </summary>
        private void DrawSelectedLayerDetails(Material material, int layerIndex, bool usingDiffuseArray, bool usingNormalArray, bool usingMaskArray)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string prefix = $"_T2M_Layer_{layerIndex}";

            bool isArrayMode = usingDiffuseArray || usingNormalArray || usingMaskArray;
            if (isArrayMode)
            {
                EditorGUILayout.HelpBox($"Layer {layerIndex} - Texture 2D Array 模式", MessageType.Info);
            }

            // Color Tint
            DrawColorPropertyCompact(material, $"{prefix}_ColorTint", "Color Tint");

            // Diffuse
            if (usingDiffuseArray)
            {
                DrawTexture2DArrayLayerPreview(material, "_T2M_DiffuseMaps2DArray", "Diffuse", layerIndex);
            }
            else
            {
                DrawTexturePropertyCompact(material, $"{prefix}_Diffuse", "Diffuse", prefix);
            }

            // Normal Scale
            DrawFloatPropertyWithUndo(material, $"{prefix}_NormalScale", "Normal Scale", 0f, 2f);

            // Normal Map
            if (usingNormalArray)
            {
                DrawTexture2DArrayLayerPreview(material, "_T2M_NormalMaps2DArray", "Normal Map", layerIndex);
            }
            else
            {
                DrawTexturePropertyCompact(material, $"{prefix}_NormalMap", "Normal Map", prefix);
            }

            // Mask Map
            if (usingMaskArray)
            {
                DrawTexture2DArrayLayerPreview(material, "_T2M_MaskMaps2DArray", "Mask Map", layerIndex);
            }
            else
            {
                DrawTexturePropertyCompact(material, $"{prefix}_Mask", "Mask Map", prefix);
            }

            // 物理属性
            Texture mask = material.GetTexture($"{prefix}_Mask");
            Texture2DArray maskArray = material.GetTexture("_T2M_MaskMaps2DArray") as Texture2DArray;

            // 检查当前layer是否有mask（通过关键字判断）
            bool layerHasMask = material.IsKeywordEnabled($"_T2M_LAYER_{layerIndex}_MASK");

            bool hasMask = false;
            if (usingMaskArray)
            {
                // 使用数组模式：检查是否有maskArray并且当前layer启用了MASK关键字
                hasMask = maskArray != null && layerHasMask;
            }
            else
            {
                // 单纹理模式：检查是否有单独的mask纹理
                hasMask = mask != null && mask != Texture2D.whiteTexture;
            }

            if (hasMask)
            {
                // 有Mask Map时显示Remap范围设置
                EditorGUILayout.LabelField("Mask Map Channels", EditorStyles.boldLabel);

                Vector4 remapMin = material.GetVector($"{prefix}_MaskMapRemapMin");
                Vector4 remapMax = material.GetVector($"{prefix}_MaskMapRemapMax");

                EditorGUI.BeginChangeCheck();

                // R: Metallic
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("R: Metallic", GUILayout.Width(EditorGUIUtility.labelWidth));
                remapMin.x = EditorGUILayout.FloatField(remapMin.x, GUILayout.Width(50));
                EditorGUILayout.MinMaxSlider(ref remapMin.x, ref remapMax.x, 0f, 1f);
                remapMax.x = EditorGUILayout.FloatField(remapMax.x, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                // G: AO
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("G: AO", GUILayout.Width(EditorGUIUtility.labelWidth));
                remapMin.y = EditorGUILayout.FloatField(remapMin.y, GUILayout.Width(50));
                EditorGUILayout.MinMaxSlider(ref remapMin.y, ref remapMax.y, 0f, 1f);
                remapMax.y = EditorGUILayout.FloatField(remapMax.y, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                // A: Smoothness
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("A: Smoothness", GUILayout.Width(EditorGUIUtility.labelWidth));
                remapMin.w = EditorGUILayout.FloatField(remapMin.w, GUILayout.Width(50));
                EditorGUILayout.MinMaxSlider(ref remapMin.w, ref remapMax.w, 0f, 1f);
                remapMax.w = EditorGUILayout.FloatField(remapMax.w, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(material, "Change Mask Remap");
                    material.SetVector($"{prefix}_MaskMapRemapMin", remapMin);
                    material.SetVector($"{prefix}_MaskMapRemapMax", remapMax);
                    EditorUtility.SetDirty(material);
                    materialEditor.PropertiesChanged();
                }
            }
            else
            {
                // 没有Mask Map时显示直接属性
                EditorGUILayout.LabelField("Physical Properties", EditorStyles.boldLabel);

                Vector4 props = material.GetVector($"{prefix}_MetallicOcclusionSmoothness");
                Vector4 newProps = props;

                EditorGUI.BeginChangeCheck();
                newProps.x = EditorGUILayout.Slider("R: Metallic", props.x, 0f, 1f);
                newProps.y = EditorGUILayout.Slider("G: AO", props.y, 0f, 1f);
                newProps.w = EditorGUILayout.Slider("A: Smoothness", props.w, 0f, 1f);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(material, "Change Physical Properties");
                    material.SetVector($"{prefix}_MetallicOcclusionSmoothness", newProps);
                    EditorUtility.SetDirty(material);
                    materialEditor.PropertiesChanged();
                }
            }

            // Tiling 和 Offset
            EditorGUI.BeginChangeCheck();

            Vector4 uvScaleOffset = material.GetVector($"{prefix}_uvScaleOffset");
            Vector2 tiling = new Vector2(uvScaleOffset.x, uvScaleOffset.y);
            Vector2 offset = new Vector2(uvScaleOffset.z, uvScaleOffset.w);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tiling", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 12;
            tiling.x = EditorGUILayout.FloatField("X", tiling.x, GUILayout.MinWidth(50));
            GUILayout.Space(5);
            tiling.y = EditorGUILayout.FloatField("Y", tiling.y, GUILayout.MinWidth(50));
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Offset", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 12;
            offset.x = EditorGUILayout.FloatField("X", offset.x, GUILayout.MinWidth(50));
            GUILayout.Space(5);
            offset.y = EditorGUILayout.FloatField("Y", offset.y, GUILayout.MinWidth(50));
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(material, "Change UV Scale/Offset");
                material.SetVector($"{prefix}_uvScaleOffset", new Vector4(tiling.x, tiling.y, offset.x, offset.y));
                EditorUtility.SetDirty(material);
                materialEditor.PropertiesChanged();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示Texture2DArray层预览（带缩略图）
        /// </summary>
        private void DrawTexture2DArrayLayerPreview(Material material, string arrayPropertyName, string label, int layerIndex)
        {
            Texture2DArray textureArray = material.GetTexture(arrayPropertyName) as Texture2DArray;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

            // 纹理预览容器（64x64）
            Rect thumbnailRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));

            EditorGUI.DrawRect(thumbnailRect, new Color(0.2f, 0.2f, 0.2f));

            if (textureArray != null && layerIndex < textureArray.depth)
            {
                Texture2D preview = GenerateArrayLayerPreview(textureArray, layerIndex, 64);
                if (preview != null)
                {
                    EditorGUI.DrawPreviewTexture(thumbnailRect, preview);
                }

                // 显示层索引
                GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
                labelStyle.alignment = TextAnchor.LowerRight;
                labelStyle.fontStyle = FontStyle.Bold;
                GUI.Label(thumbnailRect, $" [{layerIndex}] ", labelStyle);
            }

            // Locate按钮
            Rect locateRect = new Rect(thumbnailRect.x + thumbnailRect.width - 60, thumbnailRect.y + thumbnailRect.height - 18, 55, 16);
            if (GUI.Button(locateRect, "Locate", EditorStyles.miniButton))
            {
                if (textureArray != null)
                {
                    EditorGUIUtility.PingObject(textureArray);
                }
            }

            // 点击预览定位资源
            if (Event.current.type == EventType.MouseDown && thumbnailRect.Contains(Event.current.mousePosition) &&
                !locateRect.Contains(Event.current.mousePosition))
            {
                if (textureArray != null)
                {
                    EditorGUIUtility.PingObject(textureArray);
                }

                Event.current.Use();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 紧凑的纹理属性绘制（带缩略图和Select按钮）
        /// </summary>
        private void DrawTexturePropertyCompact(Material material, string propertyName, string label, string prefix)
        {
            Texture currentTexture = material.GetTexture(propertyName);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

            Rect thumbnailRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));

            EditorGUI.DrawRect(thumbnailRect, new Color(0.2f, 0.2f, 0.2f));

            if (currentTexture != null)
            {
                EditorGUI.DrawPreviewTexture(thumbnailRect, currentTexture);
            }

            Rect selectRect = new Rect(thumbnailRect.x + thumbnailRect.width - 55, thumbnailRect.y + thumbnailRect.height - 18, 50, 16);
            if (GUI.Button(selectRect, "Select", EditorStyles.miniButton))
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<Texture2D>(currentTexture, false, "", controlID);
                GUIUtility.keyboardControl = controlID;
                EditorPrefs.SetString("TexturePicker_Property", propertyName);
                EditorPrefs.SetString("TexturePicker_Prefix", prefix);
            }

            if (Event.current.type == EventType.MouseDown && thumbnailRect.Contains(Event.current.mousePosition) &&
                !selectRect.Contains(Event.current.mousePosition))
            {
                if (currentTexture != null)
                {
                    EditorGUIUtility.PingObject(currentTexture);
                }

                Event.current.Use();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete &&
                thumbnailRect.Contains(Event.current.mousePosition))
            {
                if (currentTexture != null)
                {
                    Undo.RecordObject(material, $"Clear {label}");
                    material.SetTexture(propertyName, null);
                    EditorUtility.SetDirty(material);
                    Event.current.Use();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (Event.current.commandName == "ObjectSelectorUpdated" || Event.current.commandName == "ObjectSelectorClosed")
            {
                string pickerProperty = EditorPrefs.GetString("TexturePicker_Property", "");
                string pickerPrefix = EditorPrefs.GetString("TexturePicker_Prefix", "");

                if (pickerProperty == propertyName && pickerPrefix == prefix)
                {
                    Texture newTexture = EditorGUIUtility.GetObjectPickerObject() as Texture;
                    if (newTexture != currentTexture)
                    {
                        Undo.RecordObject(material, $"Change {label}");
                        material.SetTexture(propertyName, newTexture);
                        EditorUtility.SetDirty(material);
                        GUI.changed = true;
                    }

                    if (Event.current.commandName == "ObjectSelectorClosed")
                    {
                        EditorPrefs.DeleteKey("TexturePicker_Property");
                        EditorPrefs.DeleteKey("TexturePicker_Prefix");
                    }
                }
            }
        }

        /// <summary>
        /// 紧凑的颜色属性绘制
        /// </summary>
        private void DrawColorPropertyCompact(Material material, string propertyName, string label)
        {
            Color currentColor = material.GetColor(propertyName);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            Color newColor = EditorGUILayout.ColorField(GUIContent.none, currentColor, true, true, false,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(material, $"Change {label}");
                material.SetColor(propertyName, newColor);
                EditorUtility.SetDirty(material);
            }
        }

        /// <summary>
        /// 辅助方法：绘制浮点属性（支持Undo）
        /// </summary>
        private void DrawFloatPropertyWithUndo(Material material, string propertyName, string label, float min = 0f, float max = 1f)
        {
            float currentValue = material.GetFloat(propertyName);

            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUILayout.Slider(label, currentValue, min, max);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(material, $"Change {label}");
                material.SetFloat(propertyName, newValue);
                EditorUtility.SetDirty(material);
            }
        }

        /// <summary>
        /// 创建纯色贴图（用于选中状态背景）
        /// </summary>
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}