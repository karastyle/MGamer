// <copyright file="Game.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace CarlosLab.Common
{
    public partial class Game
    {
        public bool IsPlaying => Application.isPlaying;
    }
}

#endif