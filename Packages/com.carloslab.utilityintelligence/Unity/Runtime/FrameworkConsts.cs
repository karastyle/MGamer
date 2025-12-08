// <copyright file="FrameworkRuntimeConsts.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public static class FrameworkConsts
    {
        public const string FrameworkName = "Utility Intelligence (GO)";
        public const string FrameworkVersion = "2.2.9";
        public const int DataVersion = 2;
        public const string DataExtension = "json";

        public const string AssetFileName = "New Utility Intelligence Asset";

        public const string BaseMenuPath = "Carlos Lab/" + FrameworkName + "/";
        public const string CreateAssetMenuPath = BaseMenuPath + "Utility Intelligence Asset";

        public const string AddWorldControllerMenuPath = BaseMenuPath + "Utility World Controller";

        public const string AddAgentControllerMenuPath = BaseMenuPath + "Utility Agent Controller";
        public const string AddAgentFacadeMenuPath = BaseMenuPath + "Utility Agent Facade";

        public const string AddEntityControllerMenuPath = BaseMenuPath + "Utility Entity Controller";
        public const string AddEntityFacadeMenuPath = BaseMenuPath + "Utility Entity Facade";

        public const string AddRuntimeEditorMenuPath = BaseMenuPath + "Utility Intelligence Runtime Editor";
        public const string AddRuntimeEditorPresenterMenuPath = BaseMenuPath + "Utility Intelligence Runtime Editor Presenter";
    }
}