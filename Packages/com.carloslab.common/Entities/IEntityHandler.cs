// <copyright file="IEntityHandler.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IEntityHandler
    {
        int Id { get; }
        string Name { get; }
        bool IsRegistered { get; }
        bool IsActive { get; }
        bool IsEnabled { get; }
        bool IsDestroyed { get; }
        T GetComponent<T>();
        T GetComponentInChildren<T>();

        void Register(IWorld world);
        void Register();

        void RegisterImmediate(IWorld world);
        void RegisterImmediate();
        void Unregister();
        void UnregisterImmediate();
        void SetActive(bool active);
        void Activate();
        void Deactivate();
        void SetEnable(bool enable);
        void Enable();
        void Disable();

        void SetEnableImmediate(bool enable);
        void EnableImmediate();
        void DisableImmediate();

        void Destroy();
        void DestroyAfter(float seconds);
    }
}