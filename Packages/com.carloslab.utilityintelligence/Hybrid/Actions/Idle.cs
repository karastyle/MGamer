// <copyright file="Idle.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    public class Idle : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Running;
        }
    }
}