// <copyright file="FrameworkEditorConsts.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class FrameworkEditorConsts
    {
        public const string FrameworkId = "UtilityIntelligence";
        public const string DocumentationUrl = "https://uintel-go.carloslab-ai.com/Documentation/";
        public const string DiscordUrl = "https://discord.gg/vRFEK5uE3f";
        public const string ReviewUrl = "https://links.carloslab-ai.com/8s41UZ";
        public const string EditorPrefsKey = "CarlosLab." + FrameworkId + ".EditorPrefs";
        public const string OpenWindowMenuPath = "Tools/" + FrameworkConsts.BaseMenuPath;
        public const string PackagePath = "Packages/com.carloslab.utilityintelligence/";
        public const string UIBuilderPath = PackagePath + "Unity/Editor/UIBuilder/";


        public const string GameObjectMenuPath = "GameObject/" + FrameworkConsts.BaseMenuPath;
        public const string CreateUtilityWorldMenuPath = GameObjectMenuPath + "Utility World";
        public const string UtilityWorldPrefabPath = PackagePath + "Unity/Runtime/UtilityWorld.prefab";

        public const string CreateRuntimeEditorMenuPath = "GameObject/CarlosLab/Utility Intelligence Runtime Editor";
        public const string RuntimeEditorPrefabPath = PackagePath + "Unity/UI/Views/UtilityIntelligenceView/Runtime/UtilityIntelligenceRuntimeEditor.prefab";
    }
}