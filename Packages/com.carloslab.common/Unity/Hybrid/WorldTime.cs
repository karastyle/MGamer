// <copyright file="WorldTime.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public partial class WorldTime
    {
        public int Frame => UnityEngine.Time.frameCount;

        public float Time => UnityEngine.Time.time;
    }
}