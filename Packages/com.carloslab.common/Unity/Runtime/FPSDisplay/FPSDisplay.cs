// <copyright file="FPSDisplay.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using TMPro;
using UnityEngine;

namespace CarlosLab.Common
{
    public class FPSDisplay : MonoBehaviour
    {
        private TextMeshProUGUI fpsText;
        private float deltaTime;

        private void Start()
        {
            fpsText = GetComponent<TextMeshProUGUI>();
            if (fpsText == null)
                Debug.LogWarning("FPSCounter: No TextMeshProUGUI component found. Please assign one in the inspector.");
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            UpdateFPSDisplay();
        }

        private void UpdateFPSDisplay()
        {
            if (fpsText == null) return;

            float msec = deltaTime * 1000.0f;

            float fps = 1.0f / deltaTime;

            fpsText.text = $"{msec:F1} ms ({fps:F0} fps)";
        }
    }
}