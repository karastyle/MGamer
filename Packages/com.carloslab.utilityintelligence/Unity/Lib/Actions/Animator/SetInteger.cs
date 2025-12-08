// <copyright file="SetInteger.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Animator")]
    public class SetInteger : SetParam
    {
        public VariableReference<int> Value;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetInteger(ParamName, Value);
            return UpdateStatus.Success;
        }
    }
}