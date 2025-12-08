// <copyright file="ILogger.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public interface ILogger
    {
        public void Log(string message);

        public void LogWarning(string message);

        public void LogError(string message);

        public void Log(string tag, string message);

        public void LogWarning(string tag, string message);

        public void LogError(string tag, string message);

        public void LogException(Exception exception);
    }
}