// <copyright file="Console.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Diagnostics;

namespace CarlosLab.Common
{
    public abstract class Console
    {
        private static ILogger logger;
        private static bool isInitialized;

        private readonly string tag;

        protected Console()
        {
            tag = GetTag();
        }

        public static bool IsInitialized => isInitialized;
        public bool IsEnabled { get; internal set; }

        public static void Init(ILogger newLogger)
        {
            logger = newLogger;
            isInitialized = true;
        }

        protected virtual string GetTag()
        {
            return GetType().Name;
        }

        [Conditional("CARLOSLAB_ENABLE_LOG")]
        public void Log(string message)
        {
            if (!isInitialized)
                return;

            if (!IsEnabled)
                return;

            if (string.IsNullOrEmpty(tag))
                logger.Log(message);
            else
                logger.Log(tag, message);
        }
    }

    public abstract class Console<T> : Console where T : Console<T>, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new();

                return instance;
            }
        }
    }
}