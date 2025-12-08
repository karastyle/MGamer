// <copyright file="RandomWait.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using CarlosLab.Common.Extensions;
using System;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    [Category("Wait")]
    public class RandomWait : ActionTask
    {
        private readonly Random random = new();
        private float elapsedTime;

        private float waitTime;
        public VariableReference<float> WaitTimeMax;
        public VariableReference<float> WaitTimeMin;

        protected override void OnStart()
        {
            elapsedTime = 0;
            waitTime = random.NextFloat(WaitTimeMin, WaitTimeMax);
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (elapsedTime > waitTime) return UpdateStatus.Success;
            return UpdateStatus.Running;
        }
    }
}