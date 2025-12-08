// <copyright file="CharacterView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterView : MonoBehaviour
    {
        protected void RotateTowardsCamera()
        {
            var cameraForward = Camera.main.transform.forward;
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }
    }
}