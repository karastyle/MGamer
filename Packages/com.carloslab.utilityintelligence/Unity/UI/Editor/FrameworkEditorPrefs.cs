// <copyright file="FrameworkEditorPrefs.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class FrameworkEditorPrefs
    {
        private static FrameworkEditorPrefsData data;

        private static FrameworkEditorPrefsData Data
        {
            get
            {
                if (data == null)
                    data = LoadData();

                return data;
            }
        }

        public static bool DontShowAgainWelcomeScreen
        {
            get => Data.DontShowAgainWelcomeScreen;
            set
            {
                if (Data.DontShowAgainWelcomeScreen == value)
                    return;

                Data.DontShowAgainWelcomeScreen = value;
                SaveData();
            }
        }

        public static float IntelligenceSplitView_FixedPaneWidth
        {
            get
            {
                float width = Data.IntelligenceSplitView_FixedPaneWidth;
                return width;
            }
            set
            {
                Data.IntelligenceSplitView_FixedPaneWidth = value;
                SaveData();
            }
        }

        public static float DecisionMakerSplitView_FixedPaneWidth
        {
            get
            {
                float width = Data.DecisionMakerSplitView_FixedPaneWidth;
                return width;
            }
            set
            {
                Data.DecisionMakerSplitView_FixedPaneWidth = value;
                SaveData();
            }
        }

        public static float DecisionSplitView_FixedPaneWidth
        {
            get
            {
                float width = Data.DecisionSplitView_FixedPaneWidth;
                return width;
            }
            set
            {
                Data.DecisionSplitView_FixedPaneWidth = value;
                SaveData();
            }
        }

        public static string FrameworkVersion
        {
            get => Data.FrameworkVersion;
            set
            {
                if (Data.FrameworkVersion == value)
                    return;

                Data.FrameworkVersion = value;
                SaveData();
            }
        }

        public static FrameworkEditorPrefsData LoadData()
        {
            FrameworkEditorPrefsData data;
#if UNITY_EDITOR
            string prefs = EditorPrefs.GetString(FrameworkEditorConsts.EditorPrefsKey);
#else
            string prefs = PlayerPrefs.GetString(FrameworkEditorConsts.EditorPrefsKey);
#endif
            if (!string.IsNullOrEmpty(prefs))
            {
                data = JsonConvert.DeserializeObject<FrameworkEditorPrefsData>(prefs);
            }
            else
            {
                // StaticConsole.Log("There is no EditorPrefs data. Create a new one");
                data = new();
            }

            return data;
        }

        private static void SaveData()
        {
#if UNITY_EDITOR
            EditorPrefs.SetString(FrameworkEditorConsts.EditorPrefsKey, JsonConvert.SerializeObject(Data));
#else
            PlayerPrefs.SetString(FrameworkEditorConsts.EditorPrefsKey, JsonConvert.SerializeObject(Data));
#endif
        }
    }
}