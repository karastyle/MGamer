// <copyright file="VariableValueItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableValueItemView : ValueItemView<VariableViewModel>
    {
        protected override void OnEnableEditMode()
        {
            SetValueFieldBinding();
            EnableValueField();
        }

        protected override void OnEnableRuntimeMode()
        {
            SetValueFieldBinding();
            EnableValueField();
        }
    }
}