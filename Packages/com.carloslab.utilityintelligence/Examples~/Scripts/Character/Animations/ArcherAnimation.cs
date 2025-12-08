// <copyright file="ArcherAnimation.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public enum SpineRotationState
    {
        None,
        Rotate,
        Reverse
    }
    public class ArcherAnimation : MonoBehaviour
    {
        [SerializeField]
        private Transform spineTransform;

        private SpineRotationState spineRotationState;

        float spineRotationZ = -50;

        private float totalSpineRotationDuration = 1f;
        private float spineRotationDuration = 0.35f;
        private float reverseSpineRotationDuration;

        private float spineRotationElapsedTime;

        private void Start()
        {
            reverseSpineRotationDuration = totalSpineRotationDuration - spineRotationDuration;
        }

        private void LateUpdate()
        {
            if (spineRotationState != SpineRotationState.None)
                RunSpineRotationAnimation();
        }

        private void RunSpineRotationAnimation()
        {
            float t;
            float spineRotationZ;
            Vector3 eulerAngles;
            switch (spineRotationState)
            {
                case SpineRotationState.Rotate:
                    spineRotationElapsedTime += Time.deltaTime;
                    t = spineRotationElapsedTime / spineRotationDuration;
                    if (t >= 1.0f)
                    {
                        t = 1.0f;
                        spineRotationElapsedTime = 0.0f;
                        spineRotationState = SpineRotationState.Reverse;
                    }
                    spineRotationZ = Mathf.Lerp(0, this.spineRotationZ, t);
                    eulerAngles = spineTransform.rotation.eulerAngles;
                    spineTransform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z + spineRotationZ);
                    break;
                case SpineRotationState.Reverse:
                    spineRotationElapsedTime += Time.deltaTime;

                    t = spineRotationElapsedTime / reverseSpineRotationDuration;
                    if (t >= 1.0f)
                    {
                        t = 1.0f;
                        spineRotationElapsedTime = 0.0f;
                        spineRotationState = SpineRotationState.None;
                    }

                    spineRotationZ = Mathf.Lerp(this.spineRotationZ, 0, t);
                    eulerAngles = spineTransform.rotation.eulerAngles;
                    spineTransform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z + spineRotationZ);
                    break;
            }
        }

        public void PlaySpineRotationAnimation()
        {
            spineRotationState = SpineRotationState.Rotate;
        }
    }
}