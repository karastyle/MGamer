// <copyright file="InputValueItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputValueItemView : ValueItemView<InputItemViewModel>
    {
        protected override void OnEnableEditMode()
        {
            SetValueFieldBinding();
            EnableValueField();
        }

        protected override void OnEnableRuntimeMode()
        {
            SetValueFieldBinding();
            DisableValueField();
        }
    }
}