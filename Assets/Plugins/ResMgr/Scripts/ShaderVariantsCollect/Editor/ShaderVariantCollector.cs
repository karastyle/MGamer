// ShaderVariantCollector.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace EasyShaderCollector
{
    public static class ShaderVariantCollector
    {
        private enum ESteps
        {
            None,
            Prepare,
            CollectAllMaterial,
            CollectVariants,
            CollectSleeping,
            WaitingDone,
        }

        private const float WaitMilliseconds = 3000f;
        private const float SleepMilliseconds = 3000f;
        private static string _savePath;
        private static string _tempPath;
        private static ShaderVariantCollectorConfig _collectorConfig;
        private static Action _completedCallback;

        private static ESteps _steps = ESteps.None;
        private static Stopwatch _elapsedTime;
        private static List<string> _allMaterials;
        private static List<GameObject> _allSpheres = new List<GameObject>(1000);
        private static string _originalScene;

        public static void Run(string savePath, ShaderVariantCollectorConfig config, Action completedCallback)
        {
            if (_steps != ESteps.None)
                return;

            if (Path.HasExtension(savePath) == false)
                savePath = $"{savePath}.shadervariants";
            if (Path.GetExtension(savePath) != ".shadervariants")
                throw new Exception("Shader variant file extension is invalid.");
            if (config == null)
                throw new Exception("ShaderVariantCollectorConfig is null!");
            if (config.assetBundleConfig == null)
                throw new Exception("AssetBundleConfig is null!");

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                UnityEngine.Debug.Log("用户取消收集");
                return;
            }

            _originalScene = SceneManager.GetActiveScene().path;

            EditorHelper.CreateFileDirectory(savePath);
            _savePath = savePath;
            _tempPath = savePath.Replace(".shadervariants", "_temp.shadervariants");
            _collectorConfig = config;
            _completedCallback = completedCallback;

            EditorHelper.FocusUnityGameWindow();

            _steps = ESteps.Prepare;
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            if (_steps == ESteps.None)
                return;

            if (_steps == ESteps.Prepare)
            {
                ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
                CreateTempScene();
                _steps = ESteps.CollectAllMaterial;
                return;
            }

            if (_steps == ESteps.CollectAllMaterial)
            {
                _allMaterials = GetAllMaterials();
                _steps = ESteps.CollectVariants;
                return;
            }

            if (_steps == ESteps.CollectVariants)
            {
                int count = Mathf.Min(_collectorConfig.processCapacity, _allMaterials.Count);
                List<string> range = _allMaterials.GetRange(0, count);
                _allMaterials.RemoveRange(0, count);
                CollectVariants(range);

                _elapsedTime = Stopwatch.StartNew();
                _steps = ESteps.CollectSleeping;
            }

            if (_steps == ESteps.CollectSleeping)
            {
                if (ShaderUtil.anythingCompiling)
                    return;

                if (_elapsedTime.ElapsedMilliseconds > SleepMilliseconds)
                {
                    DestroyAllSpheres();
                    _elapsedTime.Stop();
                    
                    if (_allMaterials.Count > 0)
                    {
                        _steps = ESteps.CollectVariants;
                    }
                    else
                    {
                        _elapsedTime = Stopwatch.StartNew();
                        _steps = ESteps.WaitingDone;
                    }
                }
            }

            if (_steps == ESteps.WaitingDone)
            {
                if (_elapsedTime.ElapsedMilliseconds > WaitMilliseconds)
                {
                    _elapsedTime.Stop();
                    _steps = ESteps.None;

                    // 保存到临时路径
                    AssetDatabase.DeleteAsset(_tempPath);
                    ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(_tempPath);
                    AssetDatabase.Refresh();

                    // 读取临时文件并过滤
                    ShaderVariantCollection tempCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_tempPath);
                    if (tempCollection != null)
                    {
                        // 尝试加载原有文件
                        ShaderVariantCollection targetCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_savePath);
                        bool isNewFile = false;
                        
                        if (targetCollection == null)
                        {
                            // 原文件不存在，创建新文件
                            targetCollection = new ShaderVariantCollection();
                            isNewFile = true;
                        }
                        else
                        {
                            // 原文件存在，清空内容
                            targetCollection.Clear();
                        }
                        
                        using (var so = new SerializedObject(tempCollection))
                        {
                            var shaderArray = so.FindProperty("m_Shaders.Array");
                            if (shaderArray != null && shaderArray.isArray)
                            {
                                for (int i = 0; i < shaderArray.arraySize; ++i)
                                {
                                    var shaderRef = shaderArray.FindPropertyRelative($"data[{i}].first");
                                    var shaderVariantsArray = shaderArray.FindPropertyRelative($"data[{i}].second.variants");
                                    
                                    if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference &&
                                        shaderVariantsArray != null && shaderVariantsArray.isArray)
                                    {
                                        var shader = shaderRef.objectReferenceValue as Shader;
                                        if (shader == null)
                                            continue;

                                        for (int j = 0; j < shaderVariantsArray.arraySize; ++j)
                                        {
                                            var propKeywords = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].keywords");
                                            var propPassType = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].passType");
                                            
                                            if (propKeywords != null && propPassType != null &&
                                                propKeywords.propertyType == SerializedPropertyType.String)
                                            {
                                                string keywordString = propKeywords.stringValue;
                                                PassType passType = (PassType)propPassType.intValue;

                                                ShaderVariantCollection.ShaderVariant variant = new ShaderVariantCollection.ShaderVariant();
                                                variant.shader = shader;
                                                variant.passType = passType;
                                                
                                                if (!string.IsNullOrEmpty(keywordString))
                                                {
                                                    variant.keywords = keywordString.Split(' ');
                                                }
                                                else
                                                {
                                                    variant.keywords = new string[0];
                                                }

                                                try
                                                {
                                                    if (!targetCollection.Contains(variant))
                                                    {
                                                        targetCollection.Add(variant);
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    UnityEngine.Debug.LogWarning($"跳过无效变体: {shader.name}, Pass: {passType}, Keywords: {keywordString}\n{e.Message}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // 如果是新文件才创建Asset
                        if (isNewFile)
                        {
                            EditorHelper.CreateFileDirectory(_savePath);
                            AssetDatabase.CreateAsset(targetCollection, _savePath);
                        }
                        else
                        {
                            // 标记为脏以保存修改
                            EditorUtility.SetDirty(targetCollection);
                        }
                        
                        // 删除临时文件
                        AssetDatabase.DeleteAsset(_tempPath);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("临时ShaderVariantCollection为空，使用原始收集结果");
                        if (File.Exists(_tempPath))
                        {
                            // 如果原文件不存在，直接复制
                            if (!File.Exists(_savePath))
                            {
                                File.Copy(_tempPath, _savePath, true);
                            }
                            AssetDatabase.DeleteAsset(_tempPath);
                        }
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    UnityEngine.Debug.Log($"Shader变体收集完成!");
                    int shaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
                    int variantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
                    UnityEngine.Debug.Log($"收集到 {shaderCount} 个Shader, {variantCount} 个变体");

                    CreateManifest();

                    if (!string.IsNullOrEmpty(_originalScene))
                    {
                        EditorSceneManager.OpenScene(_originalScene, OpenSceneMode.Single);
                    }

                    EditorApplication.update -= EditorUpdate;
                    _completedCallback?.Invoke();
                }
            }
        }

        private static void CreateTempScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                cameraGO.tag = "MainCamera";
                mainCamera = cameraGO.AddComponent<Camera>();
            }
            
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 10;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);

            Light light = Light.GetLights(LightType.Directional, 0).FirstOrDefault();
            if (light == null)
            {
                GameObject lightGO = new GameObject("Directional Light");
                light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
            }
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static List<string> GetAllMaterials()
        {
            int progressValue = 0;
            HashSet<string> allAssets = new HashSet<string>();

            var collectorGroups = _collectorConfig.assetBundleConfig.collectorGroups;
            int totalGroups = collectorGroups.Count;
            
            foreach (var group in collectorGroups)
            {
                if (!group.active)
                    continue;

                var collectors = group.collectors;
                foreach (var collector in collectors)
                {
                    if (collector.collectorPath == null)
                        continue;

                    string collectPath = AssetDatabase.GetAssetPath(collector.collectorPath);
                    UnityEngine.Debug.Log($"收集路径: {collectPath}");

                    if (AssetDatabase.IsValidFolder(collectPath))
                    {
                        string[] guids = AssetDatabase.FindAssets("", new[] { collectPath });
                        foreach (string guid in guids)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            if (!allAssets.Contains(assetPath))
                            {
                                allAssets.Add(assetPath);
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(collectPath) && !allAssets.Contains(collectPath))
                        {
                            allAssets.Add(collectPath);
                        }
                    }
                }
                EditorHelper.DisplayProgressBar("获取所有打包资源", ++progressValue, totalGroups);
            }
            EditorHelper.ClearProgressBar();

            progressValue = 0;
            HashSet<string> materials = new HashSet<string>();
            HashSet<string> processedAssets = new HashSet<string>();
            int totalAssets = allAssets.Count;

            foreach (string assetPath in allAssets)
            {
                ProcessAsset(assetPath, materials, processedAssets);
                EditorHelper.DisplayProgressBar("分析所有材质球", ++progressValue, totalAssets);
            }
            EditorHelper.ClearProgressBar();

            UnityEngine.Debug.Log($"一共找到 {materials.Count} 个材质球");
            return materials.ToList();
        }

        private static void ProcessAsset(string assetPath, HashSet<string> materials, HashSet<string> processedAssets)
        {
            if (processedAssets.Contains(assetPath))
                return;

            processedAssets.Add(assetPath);

            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (mainAssetType == typeof(Material))
            {
                if (!materials.Contains(assetPath))
                    materials.Add(assetPath);
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
            foreach (string dep in dependencies)
            {
                if (dep == assetPath) continue;
                if (processedAssets.Contains(dep)) continue;

                var depType = AssetDatabase.GetMainAssetTypeAtPath(dep);
                if (depType == typeof(Material))
                {
                    if (!materials.Contains(dep))
                        materials.Add(dep);
                }
            }
        }

        private static void CollectVariants(List<string> materials)
        {
            Camera camera = Camera.main;
            if (camera == null)
                throw new Exception("Not found main camera.");

            float aspect = camera.aspect;
            int totalMaterials = materials.Count;
            float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
            float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
            float halfHeight = Mathf.CeilToInt(height / 2f);
            float halfWidth = Mathf.CeilToInt(width / 2f);
            camera.orthographic = true;
            camera.orthographicSize = halfHeight;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            int xMax = (int)(width - 1);
            int x = 0, y = 0;
            int progressValue = 0;
            for (int i = 0; i < materials.Count; i++)
            {
                var material = materials[i];
                var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
                var go = CreateSphere(material, position, i);
                if (go != null)
                    _allSpheres.Add(go);
                if (x == xMax)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    x++;
                }

                EditorHelper.DisplayProgressBar("照射所有材质球", ++progressValue, materials.Count);
            }

            EditorHelper.ClearProgressBar();
        }

        private static GameObject CreateSphere(string assetPath, Vector3 position, int index)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            var shader = material.shader;
            if (shader == null)
                return null;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.transform.position = position;
            go.name = $"Sphere_{index} | {material.name}";
            return go;
        }

        private static void DestroyAllSpheres()
        {
            foreach (var go in _allSpheres)
            {
                GameObject.DestroyImmediate(go);
            }

            _allSpheres.Clear();
            EditorUtility.UnloadUnusedAssetsImmediate(true);
        }

        private static void CreateManifest()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_savePath);
            if (svc != null)
            {
                var wrapper = ShaderVariantCollectionManifest.Extract(svc);
                string jsonData = JsonUtility.ToJson(wrapper, true);
                string savePath = _savePath.Replace(".shadervariants", ".json");
                File.WriteAllText(savePath, jsonData);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}