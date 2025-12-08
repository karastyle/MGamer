// <copyright file="WorldController.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.Common
{
    [DefaultExecutionOrder(-1001)]
    public abstract class WorldController<TWorld> : MonoBehaviour
        where TWorld : IWorld
    {
        private TWorld world;

        public TWorld World => world;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (world != null) return;

            world = CreateWorld();
        }

        private void Update()
        {
            world?.Tick(Time.deltaTime);
        }

        private void OnEnable()
        {
            world?.Start();
        }

        private void OnDisable()
        {
            world?.Stop();
        }

        protected abstract TWorld CreateWorld();
    }
}