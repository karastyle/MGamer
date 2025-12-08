// <copyright file="World.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public abstract class World<TWorld, TEntity> : IWorld
        where TWorld : World<TWorld, TEntity>
        where TEntity : Entity<TEntity, TWorld>
    {
        #region Constructor

        public World()
        {
            var types = Enum.GetValues(typeof(EntityCommandType));
            foreach (EntityCommandType type in types)
            {
                entityCommandBuffer.Add(type, new HashSet<TEntity>());
            }
        }

        #endregion

        #region Fields
        private readonly HashSet<int> freeEntityIds = new();
        private readonly Queue<int> freeEntityIdQueue = new();
        private readonly Dictionary<EntityCommandType, HashSet<TEntity>> entityCommandBuffer = new();

        private List<TEntity> entityStore = new();

        protected HashSet<TEntity> enabledEntities = new();
        #endregion

        #region Poperties

        private int updateTick;
        public int UpdateTick => updateTick;

        private bool isRunning;
        public bool IsRunning => isRunning;

        public IReadOnlyCollection<TEntity> EnabledEntities => enabledEntities;


        #endregion

        #region Lifecycles

        void IWorld.Tick(float deltaTime)
        {
            if (!isRunning)
                return;

            Update(deltaTime);
            LateUpdate(deltaTime);
        }

        private void Update(float deltaTime)
        {
            updateTick++;
            OnUpdate(deltaTime);
        }

        protected virtual void OnUpdate(float deltaTime)
        {
        }

        private void LateUpdate(float deltaTime)
        {
            RunEntityCommands();

            OnLateUpdate(deltaTime);
        }

        protected virtual void OnLateUpdate(float deltaTime)
        {
        }

        public void Start()
        {
            isRunning = true;
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        public void Stop()
        {
            isRunning = false;
            OnStop();
        }

        protected virtual void OnStop()
        {
        }

        #endregion

        #region Entities

        public TEntity GetEntity(int id)
        {
            if (IdInBounds(id))
                return entityStore[id];
            return null;
        }

        public bool Contains(TEntity entity)
        {
            return entity != null && IdInBounds(entity.Id) && entityStore[entity.Id] == entity;
        }

        protected bool IdInBounds(int id)
        {
            return id >= 0 && id < entityStore.Count;
        }

        private bool TryGetFreeEntityId(out int entityId)
        {
            if (freeEntityIdQueue.Count > 0)
            {
                entityId = freeEntityIdQueue.Dequeue();
                freeEntityIds.Remove(entityId);
                return true;
            }

            entityId = -1;
            return false;
        }

        private void AddFreeEntityId(int entityId)
        {
            if (freeEntityIds.Add(entityId))
                freeEntityIdQueue.Enqueue(entityId);
        }

        private void RunEntityCommands()
        {
            foreach (var entityCommand in entityCommandBuffer)
            {
                var type = entityCommand.Key;
                var entities = entityCommand.Value;
                foreach (TEntity entity in entities)
                {
                    RunEntityCommand(type, entity);
                }
                entities.Clear();
            }
        }

        private void RunEntityCommand(EntityCommandType type, TEntity entity)
        {
            // WorldConsole.Instance.Log($"RunEntityCommand: {type} {entity.Name} Frame: {WorldTime.Instance.Frame}");
            switch (type)
            {
                case EntityCommandType.Register:
                    RegisterEntityImmediate(entity);
                    break;
                case EntityCommandType.Unregister:
                    UnregisterEntityImmediate(entity);
                    break;
                case EntityCommandType.Activate:
                    ActivateEntityImmediate(entity);
                    break;
                case EntityCommandType.Deactivate:
                    DeactivateEntityImmediate(entity);
                    break;
                case EntityCommandType.Enable:
                    EnableEntityImmediate(entity);
                    break;
                case EntityCommandType.Disable:
                    DisableEntityImmediate(entity);
                    break;
                case EntityCommandType.Destroy:
                    DestroyEntityImmediate(entity);
                    break;
            }
        }

        private void AddEntityCommand(EntityCommandType type, TEntity entity)
        {
            // WorldConsole.Instance.Log($"AddEntityCommand: {type} {entity.Name} Frame: {WorldTime.Instance.Frame}");
            entityCommandBuffer[type].Add(entity);
        }

        #endregion

        #region Register

        public void RegisterEntity(IEntity entity)
        {
            RegisterEntity(entity as TEntity);
        }

        internal void RegisterEntity(TEntity entity)
        {
            if (Contains(entity))
                return;

            if (!IsRunning)
            {
                RegisterEntityImmediate(entity);
            }
            else
            {
                AddEntityCommand(EntityCommandType.Register, entity);
            }
        }

        public void RegisterEntityImmediate(IEntity entity)
        {
            RegisterEntityImmediate(entity as TEntity);
        }

        internal void RegisterEntityImmediate(TEntity entity)
        {
            if (Contains(entity))
                return;

            if (TryGetFreeEntityId(out int entityId))
                entityStore[entityId] = entity;
            else
            {
                entityStore.Add(entity);
                entityId = entityStore.Count - 1;
            }

            RegisterEntityInternal(entity, entityId);
            EnableEntityImmediate(entity);
        }

        private void RegisterEntityInternal(TEntity entity, int entityId)
        {
            entity.OnRegistered(entityId, this as TWorld);
            OnEntityRegisteredInternal(entity);
        }

        private void OnEntityRegisteredInternal(TEntity entity)
        {
            WorldConsole.Instance.Log($"OnEntityRegistered: {entity.Name} Frame: {WorldTime.Instance.Frame}");

            OnEntityRegistered(entity);
        }

        protected virtual void OnEntityRegistered(TEntity entity)
        {
        }

        #endregion

        #region Unregister

        public void UnregisterEntity(IEntity entity)
        {
            UnregisterEntity(entity as TEntity);
        }

        internal void UnregisterEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;

            AddEntityCommand(EntityCommandType.Unregister, entity);
        }

        public void UnregisterEntityImmediate(IEntity entity)
        {
            UnregisterEntityImmediate(entity as TEntity);
        }

        internal void UnregisterEntityImmediate(TEntity entity)
        {
            if (!Contains(entity))
                return;

            int entityId = entity.Id;

            DisableEntityImmediate(entity);
            UnregisterEntityInternal(entity);

            entityStore[entityId] = null;

            AddFreeEntityId(entityId);
        }

        private void UnregisterEntityInternal(TEntity entity)
        {
            entity.OnUnregistered();

            OnEntityUnregisteredInternal(entity);
        }

        private void OnEntityUnregisteredInternal(TEntity entity)
        {
            WorldConsole.Instance.Log($"OnEntityUnregistered: {entity.Name} Frame: {WorldTime.Instance.Frame}");

            OnEntityUnregistered(entity);
        }

        protected virtual void OnEntityUnregistered(TEntity entity)
        {
        }

        #endregion

        #region Activate

        internal void ActivateEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;

            AddEntityCommand(EntityCommandType.Activate, entity);
        }

        private void ActivateEntityImmediate(TEntity entity)
        {
            if (!Contains(entity))
                return;

            ActivateEntityInternal(entity);

            EnableEntityImmediate(entity);
        }

        private void ActivateEntityInternal(TEntity entity)
        {
            entity.ActivateInternal();
        }

        #endregion

        #region Deactivate

        internal void DeactivateEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;

            AddEntityCommand(EntityCommandType.Deactivate, entity);
        }

        private void DeactivateEntityImmediate(TEntity entity)
        {
            if (!Contains(entity))
                return;

            DisableEntityImmediate(entity);

            DeactivateEntityInternal(entity);
        }

        private void DeactivateEntityInternal(TEntity entity)
        {
            entity.DeactivateInternal();
        }

        #endregion

        #region Enable

        internal void EnableEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;

            if (entity.IsEnabling) return;
            entity.IsEnabling = true;

            AddEntityCommand(EntityCommandType.Enable, entity);
        }

        internal void EnableEntityImmediate(TEntity entity)
        {
            if (!Contains(entity))
                return;

            if (enabledEntities.Add(entity))
                EnableEntityInternal(entity);
        }

        private void EnableEntityInternal(TEntity entity)
        {
            entity.OnEnabled();
            OnEntityEnabledInternal(entity);
        }

        private void OnEntityEnabledInternal(TEntity entity)
        {
            WorldConsole.Instance.Log($"OnEntityEnabled: {entity.Name} Frame: {WorldTime.Instance.Frame}");
            entity.IsEnabling = false;
            OnEntityEnabled(entity);
        }

        protected virtual void OnEntityEnabled(TEntity entity)
        {
        }

        #endregion

        #region Disable

        internal void DisableEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;

            if (entity.IsDisabling) return;
            entity.IsDisabling = true;

            AddEntityCommand(EntityCommandType.Disable, entity);
        }

        internal void DisableEntityImmediate(TEntity entity)
        {
            if (!Contains(entity))
                return;

            if (enabledEntities.Remove(entity))
                DisableEntityInternal(entity);
        }

        private void DisableEntityInternal(TEntity entity)
        {
            entity.OnDisabled();
            OnEntityDisabledInternal(entity);
        }

        private void OnEntityDisabledInternal(TEntity entity)
        {
            WorldConsole.Instance.Log($"OnEntityDisabled: {entity.Name} Frame: {WorldTime.Instance.Frame}");
            entity.IsDisabling = false;
            OnEntityDisabled(entity);
        }

        protected virtual void OnEntityDisabled(TEntity entity)
        {
        }

        #endregion

        #region Destroy

        internal void DestroyEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;

            AddEntityCommand(EntityCommandType.Destroy, entity);
        }

        private void DestroyEntityImmediate(TEntity entity)
        {
            if (!Contains(entity))
                return;

            UnregisterEntityImmediate(entity);
            DestroyEntityInternal(entity);
        }

        private void DestroyEntityInternal(TEntity entity)
        {
            entity.DestroyInternal();
        }

        #endregion

    }
}