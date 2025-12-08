// <copyright file="DebugUtils.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.Common
{
    public static class DebugUtils
    {
        public static void DrawPoint(Vector3 position, Color color, float scale = 1.0f, float duration = 0, bool depthTest = true)
        {
            DrawRay(position, Vector3.up, color, scale, duration, depthTest);
            DrawRay(position, Vector3.right, color, scale, duration, depthTest);
            DrawRay(position, Vector3.forward, color, scale, duration, depthTest);
        }

        private static void DrawRay(Vector3 position, Vector3 direction, Color color, float scale = 1.0f, float duration = 0, bool depthTest = true)
        {
            position += (direction * scale * 0.5f);
            direction = -direction * scale;
            Debug.DrawRay(position, direction, color, duration, depthTest);
        }
    }
}