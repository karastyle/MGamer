// <copyright file="SetTrigger.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Animator")]
    public class SetTrigger : SetParam
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetTrigger(ParamName);
            return UpdateStatus.Success;
        }
    }
}