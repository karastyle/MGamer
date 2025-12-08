// <copyright file="FrameworkEditorStarter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEditor;

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class FrameworkEditorStarter
    {
        [InitializeOnLoadMethod]
        private static void StartUp()
        {
            WelcomeScreenWindow.StartUp();
        }
    }
}