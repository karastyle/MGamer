// <copyright file="UtilityIntelligenceEditor.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Editor;
using CarlosLab.UtilityIntelligence.UI;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CarlosLab.UtilityIntelligence.Editor
{
    public class UtilityIntelligenceEditor : BaseEditorWindow
    {
        private bool isLocked;

        private GUIStyle lockButtonStyle;

        public GUIStyle LockButtonStyle
        {
            get
            {
                if (lockButtonStyle == null)
                    lockButtonStyle = GUI.skin.FindStyle("IN LockButton");

                return lockButtonStyle;
            }
        }

        private void ShowButton(Rect position)
        {
            EditorGUI.BeginChangeCheck();
            bool newLock = GUI.Toggle(position, isLocked, GUIContent.none, LockButtonStyle);
            if (EditorGUI.EndChangeCheck())
            {
                if (newLock != isLocked)
                {
                    isLocked = !isLocked;
                    UpdateView();
                }
            }
        }

        #region Window Functions

        public static void OpenWindow(UtilityIntelligenceAsset asset)
        {
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Error!", "Cannot open Intelligence Editor Window! The asset is null",
                    "OK");
                return;
            }

            OpenWindow();
        }

        [MenuItem(UtilityIntelligenceEditorConsts.MenuPath)]
        public static void OpenWindow()
        {
            GetWindow<UtilityIntelligenceEditor>(false, FrameworkConsts.FrameworkName);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            Object openAsset = EditorUtility.InstanceIDToObject(instanceId);
            if (openAsset is UtilityIntelligenceAsset asset)
            {
                OpenWindow(asset);
                return true;
            }

            return false;
        }

        #endregion

        #region UtilityIntelligenceEditor

        private UtilityIntelligenceViewController viewController;

        protected override void OnInitGUI()
        {
            minSize = new Vector2(600, 300);

            LoadVisualAsset(UtilityIntelligenceEditorConsts.VisualAssetPath);
        }

        // InitView();
        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            InitToolBarMenu();

            if (EditorGUIUtility.isProSkin)
                LoadStyleSheet(UtilityIntelligenceEditorConsts.DarkTheme);
            else
                LoadStyleSheet(UtilityIntelligenceEditorConsts.LightTheme);

            InitView();

            UpdateView(false, true);
        }

        private void InitView()
        {
            viewController = new(false);
            rootVisualElement.Add(viewController.View);
        }

        private void CloseEditor()
        {
            viewController?.CloseEditor();
        }

        private void UpdateView(bool updateModel = false, bool checkDataVersion = false)
        {
            if (isLocked)
                return;

            bool result = UpdateViewIfSelectedObjectIsIntelligenceAsset(updateModel, checkDataVersion);

            if (!result)
                result = UpdateViewIfSelectedObjectIsAgentOwner(updateModel, checkDataVersion);

            if (result)
                return;

            CloseEditor();
        }

        private bool UpdateViewIfSelectedObjectIsIntelligenceAsset(bool updateModel, bool checkDataVersion)
        {
            if (Selection.activeObject is UtilityIntelligenceAsset asset)
            {
                UpdateView(asset.name, asset, updateModel, checkDataVersion);

                return true;
            }

            return false;
        }

        private bool UpdateViewIfSelectedObjectIsAgentOwner(bool updateModel, bool checkDataVersion)
        {
            if (Selection.activeGameObject != null)
            {
                UtilityAgentController agentController = Selection.activeGameObject.GetComponent<UtilityAgentController>();
                if (agentController != null && agentController.Asset != null)
                {
                    UpdateView(agentController.Name, agentController.Asset, updateModel, checkDataVersion);

                    return true;
                }
            }

            return false;
        }

        private void UpdateView(string name, UtilityIntelligenceAsset asset, bool updateModel, bool checkDataVersion)
        {
            if (viewController == null || asset == null)
                return;

            dataVersionLabel.text = $"Data Version: {asset.DataVersion}";

            if (checkDataVersion)
            {
                if (!asset.IsDataVersionValid())
                {
                    // Debug.Log("Data Version is not valid!");
                    asset.ShowDataVersionNotCompatiblePopup();
                }
            }

            var viewModel = viewController.ViewModel;

            if (viewModel != null && viewModel.Asset == asset)
            {
                if (updateModel) UpdateModel(asset, viewModel);
            }
            else
            {
                viewModel = viewController.CreateViewModel(asset);
            }

            viewModel.Name = name;

            viewController.UpdateView(viewModel);
        }

        private static void UpdateModel(UtilityIntelligenceAsset intelligenceAsset, UtilityIntelligenceViewModel viewModel)
        {
            UtilityIntelligenceConsole.Instance.Log("UpdateModel");
            intelligenceAsset.BlockRecording = true;

            intelligenceAsset.ResetModel();
            viewModel.Model = intelligenceAsset.Model;

            intelligenceAsset.BlockRecording = false;
        }

        #endregion

        #region Event Functions

        protected override void OnDisable()
        {
            base.OnDisable();
            CloseEditor();
        }

        protected override void OnUndoRedo()
        {
            var asset = viewController.Asset;
            if (asset == null || asset.IsRuntime)
                return;

            asset.IsInUndoRedo = true;
            UpdateView(true);
            asset.IsInUndoRedo = false;
        }

        protected override void OnEnterEditMode()
        {
            isLocked = false;
            UpdateView();
        }

        protected override void OnEnterPlayerMode()
        {
            isLocked = false;

            UpdateView();
        }

        //This will not be called when the focus is lost
        private void OnSelectionChange()
        {
            UpdateView(false, true);
        }

        //Update View when the focus is gained
        private void OnFocus()
        {
            UpdateView();
        }

        #endregion

        #region Toolbar

        private ToolbarMenu fileToolbarMenu;
        private Label frameworkVersionLabel;
        private Label dataVersionLabel;

        private void InitToolBarMenu()
        {
            frameworkVersionLabel = rootVisualElement.Q<Label>("FrameworkVersionLabel");
            dataVersionLabel = rootVisualElement.Q<Label>("DataVersionLabel");

            fileToolbarMenu = rootVisualElement.Q<ToolbarMenu>("FileToolbarMenu");
            fileToolbarMenu.menu.AppendAction("Import Data", OnFileToolBarMenu_ImportData);
            fileToolbarMenu.menu.AppendAction("Export Data", OnToolBarMenu_ExportData);
            fileToolbarMenu.menu.AppendAction("Show Data", OnToolBarMenu_ShowData);
            fileToolbarMenu.menu.AppendAction("Clear Data", OnToolBarMenu_ClearData);
        }

        private void OnFileToolBarMenu_ImportData(DropdownMenuAction action)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("You cannot import data while playing");
                return;
            }

            var asset = viewController.Asset;
            if (asset == null)
            {
                EditorUtility.DisplayDialog("The Utility Intelligence Asset is empty!",
                    "You have not selected a Utility Intelligence Asset to import data. Please select one to continue.", "OK");
                return;
            }

            string filePath = EditorUtility.OpenFilePanel("Import Data"
                , Application.persistentDataPath
                , FrameworkConsts.DataExtension);

            if (string.IsNullOrEmpty(filePath)) return;

            if (!EditorUtility.DisplayDialog("Import Data", "All current data will be lost. Are you sure?", "YES",
                    "NO"))
                return;

            string serializedModel = File.ReadAllText(filePath);

            asset.ImportModel(serializedModel);

            var viewModel = viewController.CreateViewModel(asset);
            viewController.UpdateView(viewModel);
        }

        private void OnToolBarMenu_ExportData(DropdownMenuAction action)
        {
            var asset = viewController.Asset;
            if (asset == null)
            {
                EditorUtility.DisplayDialog("The Utility Intelligence Asset is empty!",
                    "You have not selected a Utility Intelligence Asset to export data. Please select one to continue.", "OK");
                return;
            }

            string filePath = EditorUtility.SaveFilePanelInProject("Export Data"
                , asset.Name
                , FrameworkConsts.DataExtension
                , string.Empty);

            if (!string.IsNullOrEmpty(filePath))
            {
                asset.SerializeModel();
                string formattedJson = asset.FormattedSerializedModel;
                File.WriteAllText(filePath, formattedJson);
                AssetDatabase.Refresh();
            }
        }

        private void OnToolBarMenu_ShowData(DropdownMenuAction action)
        {
            var asset = viewController.Asset;
            if (asset == null)
            {
                EditorUtility.DisplayDialog("The Utility Intelligence Asset is empty!",
                    "You have not selected a Utility Intelligence Asset to show data. Please select one to continue.", "OK");
                return;
            }

            asset.SerializeModel();
            string formattedJson = asset.FormattedSerializedModel;
            string filePath = $"{Application.persistentDataPath}/{asset.Name}.json";
            File.WriteAllText(filePath, formattedJson);
            Process.Start(filePath);
        }

        private void OnToolBarMenu_ClearData(DropdownMenuAction action)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("You cannot clear data while playing");
                return;
            }
            var asset = viewController.Asset;
            if (asset == null)
            {
                EditorUtility.DisplayDialog("The Utility Intelligence Asset is empty!",
                    "You have not selected a Utility Intelligence Asset to clear data. Please select one to continue.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Clear Data", "This will clear all current data! Are you sure?", "YES",
                    "NO"))
                return;

            asset.ClearModel();
            CloseEditor();
            UpdateView(asset.Name, asset, false, false);
        }

        #endregion

        // private void OnGUI()
        // {
        //     Debug.Log("OnGUI Current Size: " + position);
        // }
    }
}