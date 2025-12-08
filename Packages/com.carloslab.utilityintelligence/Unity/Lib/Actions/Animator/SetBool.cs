// <copyright file="SetBool.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Animator")]
    public class SetBool : SetParam
    {
        public VariableReference<bool> Value;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetBool(ParamName, Value);
            return UpdateStatus.Success;
        }
    }
}