// <copyright file="StraightProjectile.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class StraightProjectile : BaseProjectile
    {
        protected override void OnStartAttack(Vector3 targetPos)
        {
            transform.LookAt(targetPos, Vector3.up);

            var direction = targetPos - transform.position;
            direction.Normalize();
            var velocity = direction * speed;

#if UNITY_6000_0_OR_NEWER
            rigidBody.linearVelocity = velocity;
#else
            rigidBody.velocity = velocity;
#endif
        }
    }
}