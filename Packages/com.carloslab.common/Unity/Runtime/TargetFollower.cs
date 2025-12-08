// <copyright file="TargetFollower.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.Common
{
    public class TargetFollower : MonoBehaviour
    {
        private Transform target;

        private void LateUpdate()
        {
            if (target != null)
                transform.position = target.transform.position;
        }

        public void FollowTarget(Transform target)
        {
            this.target = target;
        }
    }
}