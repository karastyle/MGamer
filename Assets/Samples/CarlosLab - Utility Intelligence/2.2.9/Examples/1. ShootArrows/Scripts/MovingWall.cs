// <copyright file="MovingWall.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class MovingWall : MonoBehaviour
    {
        [SerializeField]
        private float maxDistance;

        [SerializeField]
        private float speed;

        private Vector3 startPos;

        private bool movingRight;

        private void Start()
        {
            movingRight = true;
            startPos = transform.position;
        }

        private void Update()
        {
            Vector3 direction = movingRight ? transform.right : -transform.right;
            transform.position += direction * speed * Time.deltaTime;
            var currentPos = transform.position;
            float distance = Vector3.Distance(startPos, currentPos);

            if (distance >= maxDistance)
            {
                movingRight = !movingRight;
            }
        }
    }
}
