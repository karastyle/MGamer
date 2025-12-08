// <copyright file="DestroySelf.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    public class DestroySelf : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            Agent.Destroy();
            return UpdateStatus.Success;
        }
    }
}