// <copyright file="Log.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Lib
{
    [ClassFormerlySerializedAs(oldNamespace: "CarlosLab.UtilityIntelligence")]
    public class Log : ActionTask
    {
        public string Message;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            Debug.Log($"LogTask Message: {Message}");
            return UpdateStatus.Success;
        }
    }
}