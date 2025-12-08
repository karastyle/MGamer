// <copyright file="UIElementsFactory.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public static class UIElementsFactory
    {
        public static Label CreateTitleLabel(string title = "")
        {
            Label label = new(title);
            label.AddToClassList("title-label");
            return label;
        }
    }
}