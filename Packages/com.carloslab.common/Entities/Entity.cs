// <copyright file="Entity.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public abstract class Entity<TEntity, TWorld> : IEntity<TWorld>
        where TWorld : World<TWorld, TEntity>
        where TEntity : Entity<TEntity, TWorld>
    {
        #region EntityFacade

        private IEntityFacade entityFacade;

        IEntityFacade IEntity.EntityFacade
        {
            get => entityFacade;
            set => entityFacade = value;
        }

        public IEntityFacade EntityFacade => entityFacade;

        #endregion

        #region Entity

        private int id = -1;

        public int Id => id;

        private string name;

        string IEntity.Name
        {
            get => name;
            set => name = value;
        }

        public string Name => name;

        public T GetComponent<T>()
        {
            return entityFacade != null ? entityFacade.GetComponent<T>() : default;
        }

        public T GetComponentInChildren<T>()
        {
            return entityFacade != null ? entityFacade.GetComponentInChildren<T>() : default;
        }

        #endregion

        #region World

        private TWorld world;
        public TWorld World => world;

        #endregion

        #region Register/Unregister

        private bool isRegistered;
        public bool IsRegistered => isRegistered;

        public bool CanRegister => !isRegistered && isActive;

        private bool CanUnregister => isRegistered;

        public void Register(IWorld world)
        {
            Register(world as TWorld);
        }

        public void Register(TWorld world)
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Register(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanRegister)
                return;

            world.RegisterEntity(this as TEntity);
        }

        public void RegisterImmediate(IWorld world)
        {
            RegisterImmediate(world as TWorld);
        }

        public void RegisterImmediate(TWorld world)
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Register(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanRegister)
                return;

            world.RegisterEntityImmediate(this as TEntity);
        }

        public void Register()
        {
            Register(world);
        }

        public void RegisterImmediate()
        {
            RegisterImmediate(world);
        }

        internal void OnRegistered(int id, TWorld world)
        {
            this.id = id;
            this.world = world;
            isRegistered = true;
            entityFacade?.OnRegistered();
        }

        public void Unregister()
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Unregister(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanUnregister)
                return;

            world.UnregisterEntity(this as TEntity);
        }

        public void UnregisterImmediate()
        {
            if (world == null)
            {
                // StaticConsole.LogWarning($"Unregister(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanUnregister)
                return;

            world.UnregisterEntityImmediate(this as TEntity);
        }

        internal void OnUnregistered()
        {
            id = -1;
            isRegistered = false;
            entityFacade?.OnUnregistered();
        }

        #endregion

        #region Activate/Deactivate


        private bool isActive;
        public bool IsActive => isActive;

        public void SetActive(bool active)
        {
            if (active)
                Activate();
            else
                Deactivate();
        }

        public void Activate()
        {
            if (isActive) return;

            if (!isRegistered)
            {
                ActivateInternal();
                return;
            }

            if (world == null)
            {
                StaticConsole.LogWarning($"Activate(): World is null - Entity Name: {Name}");
                return;
            }

            world.ActivateEntity(this as TEntity);
        }

        public void ActivateImmediate()
        {
            if (isActive) return;

            ActivateInternal();
            EnableImmediate();
        }

        internal void ActivateInternal()
        {
            entityFacade?.ActivateInternal();
        }

        private void OnActivated()
        {
            isActive = true;
            entityFacade?.OnActivated();
        }

        void IEntity.OnActivated()
        {
            OnActivated();
            EnableImmediate();
        }

        public void Deactivate()
        {
            if (!isActive) return;

            if (!isRegistered)
            {
                DeactivateInternal();
                return;
            }

            if (world == null)
            {
                StaticConsole.LogWarning($"Deactivate(): World is null - Entity Name: {Name}");
                return;
            }

            world.DeactivateEntity(this as TEntity);
        }

        public void DeactivateImmediate()
        {
            if (!isActive) return;

            DisableImmediate();
            DeactivateInternal();
        }

        internal void DeactivateInternal()
        {
            entityFacade?.DeactivateInternal();
        }

        private void OnDeactivated()
        {
            isActive = false;
            entityFacade?.OnDeactivated();
        }

        void IEntity.OnDeactivated()
        {
            DisableImmediate();
            OnDeactivated();
        }

        #endregion

        #region Enable/Disable

        internal bool IsEnabling;
        internal bool IsDisabling;

        private bool isEnabled;
        public bool IsEnabled => isEnabled;

        private bool CanEnable => isRegistered && isActive && !isEnabled;

        private bool CanDisable => isRegistered && isEnabled;

        public void SetEnable(bool enabled)
        {
            if (enabled)
                Enable();
            else
                Disable();
        }

        public void Enable()
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Enable(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanEnable)
                return;

            world.EnableEntity(this as TEntity);
        }

        public void Disable()
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Disable(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanDisable)
                return;

            world.DisableEntity(this as TEntity);
        }

        public void SetEnableImmediate(bool enabled)
        {
            if (enabled)
                EnableImmediate();
            else
                DisableImmediate();
        }

        public void EnableImmediate()
        {
            if (world == null)
            {
                // StaticConsole.LogWarning($"Enable(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanEnable)
                return;

            world.EnableEntityImmediate(this as TEntity);
        }

        public void DisableImmediate()
        {
            if (world == null)
            {
                // StaticConsole.LogWarning($"Disable(): World is null - Entity Name: {Name}");
                return;
            }

            if (!CanDisable)
                return;

            world.DisableEntityImmediate(this as TEntity);
        }

        internal void OnEnabled()
        {
            isEnabled = true;

            entityFacade?.OnEnabled();
        }

        internal void OnDisabled()
        {
            isEnabled = false;

            entityFacade?.OnDisabled();
        }

        #endregion

        #region Destroy

        private bool isDestroyed;

        public bool IsDestroyed => isDestroyed;

        public void Destroy()
        {
            if (isDestroyed)
                return;

            if (!isRegistered)
            {
                DestroyInternal();
                return;
            }

            if (world == null)
            {
                StaticConsole.LogWarning($"Destroy(): World is null - Entity Name: {Name}");
                return;
            }

            world.DestroyEntity(this as TEntity);
        }

        public void DestroyImmediate()
        {
            if (isDestroyed)
                return;

            UnregisterImmediate();
            DestroyInternal();
        }

        internal void DestroyInternal()
        {
            entityFacade?.DestroyInternal();
        }

        private void OnDestroyed()
        {
            isDestroyed = true;
            entityFacade?.OnDestroyed();
            entityFacade = null;
        }

        void IEntity.OnDestroyed()
        {
            UnregisterImmediate();
            OnDestroyed();
        }

        #endregion
    }
}