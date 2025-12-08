// <copyright file="IEntity.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IEntity
    {
        int Id { get; }

        string Name { get; internal set; }

        IEntityFacade EntityFacade { get; internal set; }
        bool IsRegistered { get; }
        bool IsActive { get; }
        bool IsEnabled { get; }
        bool IsDestroyed { get; }

        bool CanRegister { get; }

        T GetComponent<T>();
        T GetComponentInChildren<T>();

        void Register(IWorld world);
        void Register();

        void RegisterImmediate(IWorld world);
        void RegisterImmediate();
        void Unregister();
        void UnregisterImmediate();

        void SetEnable(bool enabled);
        void Enable();
        void Disable();

        void SetEnableImmediate(bool enabled);
        void EnableImmediate();
        void DisableImmediate();

        void SetActive(bool active);
        void Activate();
        void Deactivate();
        void Destroy();
        internal void OnActivated();
        internal void OnDeactivated();
        internal void OnDestroyed();
    }

    public interface IEntity<TWorld> : IEntity where TWorld : class, IWorld
    {
        TWorld World { get; }

        void Register(TWorld world);
    }
}