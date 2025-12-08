// <copyright file="UnityLogger.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine;

namespace CarlosLab.Common
{
    public class UnityLogger : ILogger
    {
        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        public void LogError(string message)
        {
            Debug.LogError(message);
        }

        public void Log(string tag, string message)
        {
            Debug.unityLogger.Log(tag, message);
        }

        public void LogWarning(string tag, string message)
        {
            Debug.unityLogger.LogWarning(tag, message);
        }

        public void LogError(string tag, string message)
        {
            Debug.unityLogger.LogError(tag, message);
        }

        public void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}