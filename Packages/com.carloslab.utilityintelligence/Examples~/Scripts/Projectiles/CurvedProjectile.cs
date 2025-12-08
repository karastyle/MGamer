// <copyright file="CurvedProjectile.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CurvedProjectile : BaseProjectile
    {
        private Vector3 startVelocityXZ;
        private float startVelocityY;

        private float elapsedTime;

        private float maxCurvedHeight = 3.5f;

        public void StartAttack(Vector3 targetPos, int attackDamage, float speed, Team team, float maxCurvedHeight)
        {
            this.maxCurvedHeight = maxCurvedHeight;
            StartAttack(targetPos, attackDamage, speed, team);
        }

        protected override void OnStartAttack(Vector3 targetPos)
        {
            Vector3 diff = targetPos - transform.position;
            float diffY = diff.y;
            if (diffY > maxCurvedHeight)
                maxCurvedHeight = diffY;

            Vector3 diffXZ = new(diff.x, 0, diff.z);

            var flightTime = Mathf.Sqrt(2 * maxCurvedHeight / speed) + Mathf.Sqrt(2 * (maxCurvedHeight - diffY) / speed);
            startVelocityY = Mathf.Sqrt(2 * speed * maxCurvedHeight);
            startVelocityXZ = diffXZ / flightTime;

            Vector3 velocity = new(startVelocityXZ.x, startVelocityY, startVelocityXZ.z);
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        private void FixedUpdate()
        {
            if (isAttacking)
            {
                elapsedTime += Time.fixedDeltaTime;

                float velocityY = startVelocityY - speed * elapsedTime;
                Vector3 velocity = new(startVelocityXZ.x, velocityY, startVelocityXZ.z);
                transform.rotation = Quaternion.LookRotation(velocity);

#if UNITY_6000_0_OR_NEWER
                rigidBody.linearVelocity = velocity;
#else
                rigidBody.velocity = velocity;
#endif
            }
        }

        protected override void OnStopAttack()
        {
            elapsedTime = 0;
        }
    }
}