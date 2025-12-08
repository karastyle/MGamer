// <copyright file="Wait.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Wait")]
    public class Wait : ActionTask
    {
        private float elapsedTime;
        public VariableReference<float> WaitTime = 1.0f;

        protected override void OnStart()
        {
            elapsedTime = 0;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (elapsedTime > WaitTime) return UpdateStatus.Success;
            return UpdateStatus.Running;
        }
    }
}