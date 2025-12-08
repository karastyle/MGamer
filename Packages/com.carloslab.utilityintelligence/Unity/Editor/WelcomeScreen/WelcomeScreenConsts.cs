// <copyright file="WelcomeScreenConsts.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class WelcomeScreenConsts
    {
        public const string MenuPath = FrameworkEditorConsts.OpenWindowMenuPath + "/Welcome Screen";
        public const string VisualAssetPath = FrameworkEditorConsts.UIBuilderPath + "WelcomeScreen.uxml";
        public const string LightThemePath = FrameworkEditorConsts.UIBuilderPath + "WelcomeScreen_Light.uss";
        public const string DarkThemePath = FrameworkEditorConsts.UIBuilderPath + "WelcomeScreen_Dark.uss";

        public static readonly string WelcomeLabelText = $"Welcome to Utility Intelligence";
        public const string ThanksLabelText = "Thank you for choosing us! 🥰";

        public static readonly string FrameworkVersionLabelText = $"Framework Version: {FrameworkConsts.FrameworkVersion}";

        public static readonly string SupportLabelText = @$"If you <b>haven’t</b> already, please <b>consider</b> leaving a <b>review</b> on the <a href={FrameworkEditorConsts.ReviewUrl}><b>the Asset Store</b></a>. Whether <b>good</b> or <b>bad</b>, your <b>feedback</b> helps <b>shape</b> the <b>future</b> of this framework, and lets others <b>determine</b> whether it’s a <b>good fit</b> for their <b>games</b>.<br><br><b>Thank</b> you <b>so much</b>!💘 I <b>love</b> you all!🥰";
    }
}