using System;
using System.Reflection;
using UnityEditor;

public static class EditorHelper
{
    public static object InvokeNonPublicStaticMethod(Type type, string method, params object[] parameters)
    {
        var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
        if (methodInfo == null)
        {
            UnityEngine.Debug.LogError($"Method not found: {type.FullName}.{method}");
            return null;
        }
        return methodInfo.Invoke(null, parameters);
    }

    public static void FocusUnityGameWindow()
    {
        var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        EditorWindow.GetWindow(gameViewType, false, null, true);
    }

    public static void DisplayProgressBar(string title, int progressValue, int totalValue)
    {
        float progress = (float)progressValue / totalValue;
        EditorUtility.DisplayProgressBar(title, $"{progressValue}/{totalValue}", progress);
    }

    public static void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

    public static void CreateFileDirectory(string filePath)
    {
        string directory = System.IO.Path.GetDirectoryName(filePath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
    }
}