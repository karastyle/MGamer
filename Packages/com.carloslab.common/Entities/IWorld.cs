// <copyright file="IWorld.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IWorld
    {
        int UpdateTick { get; }

        bool IsRunning { get; }

        internal void Tick(float deltaTime);

        void Start();
        void Stop();

        void RegisterEntity(IEntity entity);

        void RegisterEntityImmediate(IEntity entity);

        void UnregisterEntity(IEntity entity);

        void UnregisterEntityImmediate(IEntity entity);
    }
}