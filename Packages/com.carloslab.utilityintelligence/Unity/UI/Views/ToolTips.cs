// <copyright file="ToolTips.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public static class ToolTips
    {
        public const string Reorderable = "Enable the ability to reorder the list";

        public const string KeepRunningUntilFinished =
            "Prevent the agent from making a new decision while the action list is running";

        public const string MaxRepeatCount = "The number of times to repeat the action list. Note: if MaxRepeatCount <= 0 it will be repeated forever";

        public const string CurrentRepeatCount = "The number of times the action list has been run";

    }
}