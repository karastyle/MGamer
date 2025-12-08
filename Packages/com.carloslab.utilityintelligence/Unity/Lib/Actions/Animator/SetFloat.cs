// <copyright file="SetFloat.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Animator")]
    public class SetFloat : SetParam
    {
        public VariableReference<float> Value;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetFloat(ParamName, Value);
            return UpdateStatus.Success;
        }
    }
}