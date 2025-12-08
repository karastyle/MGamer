// <copyright file="EntityFacade.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections;
using UnityEngine;

namespace CarlosLab.Common
{
    public abstract class EntityFacade<TEntity> : MonoBehaviour, IEntityFacade
        where TEntity : class, IEntity
    {
        #region Entity Properties

        private TEntity entity;

        IEntity IEntityFacade.Entity
        {
            get => entity;
            set => entity = value as TEntity;
        }

        public TEntity Entity
        {
            get => entity;
            internal set => entity = value;
        }

        public int Id => entity?.Id ?? -1;

        public string Name => name;

        public bool IsRegistered => entity is { IsRegistered: true };
        public bool IsActive => entity is { IsActive: true };
        public bool IsEnabled => entity is { IsEnabled: true };
        public bool IsDestroyed => entity is { IsDestroyed: true };

        public Float3 Position => transform.position;

        #endregion

        #region Entity Methods

        public void Register(IWorld world)
        {
            entity?.Register(world);
        }

        public void Register()
        {
            entity?.Register();
        }

        public void RegisterImmediate(IWorld world)
        {
            entity?.RegisterImmediate(world);
        }

        public void RegisterImmediate()
        {
            entity?.RegisterImmediate();
        }

        public void Unregister()
        {
            entity?.Unregister();
        }

        public void UnregisterImmediate()
        {
            entity?.UnregisterImmediate();
        }

        public void SetActive(bool active)
        {
            entity?.SetActive(active);
        }

        public void Activate()
        {
            entity?.Activate();
        }

        public void Deactivate()
        {
            entity?.Deactivate();
        }

        public void SetEnable(bool enabled)
        {
            entity?.SetEnable(enabled);
        }

        public void Enable()
        {
            entity?.Enable();
        }

        public void Disable()
        {
            entity?.Disable();
        }

        public void SetEnableImmediate(bool enable)
        {
            entity?.SetEnableImmediate(enable);
        }

        public void EnableImmediate()
        {
            entity?.EnableImmediate();
        }

        public void DisableImmediate()
        {
            entity?.DisableImmediate();
        }

        public void DestroyAfter(float seconds)
        {
            if (seconds > 0)
                StartCoroutine(DestroyAfterCoroutine(seconds));
            else
                Destroy();
        }

        private IEnumerator DestroyAfterCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy();
        }

        public void Destroy()
        {
            entity?.Destroy();
        }

        void IEntityFacade.DestroyInternal()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }

        void IEntityFacade.ActivateInternal()
        {
            gameObject.SetActive(true);
        }

        void IEntityFacade.DeactivateInternal()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region Event Functions

        void IEntityFacade.OnRegistered()
        {
            EntityConsole.Instance.Log($"OnRegistered: {Name} Frame: {WorldTime.Instance.Frame}");

            OnRegistered();
        }

        void IEntityFacade.OnUnregistered()
        {
            EntityConsole.Instance.Log($"OnUnregistered: {Name} Frame: {WorldTime.Instance.Frame}");

            OnUnregistered();
        }

        void IEntityFacade.OnActivated()
        {
            EntityConsole.Instance.Log($"OnActivated: {Name} Frame: {WorldTime.Instance.Frame}");

            OnActivated();
        }

        void IEntityFacade.OnDeactivated()
        {
            EntityConsole.Instance.Log($"OnDeactivated: {Name} Frame: {WorldTime.Instance.Frame}");
            OnDeactivated();
        }

        void IEntityFacade.OnEnabled()
        {
            EntityConsole.Instance.Log($"OnEnabled: {Name} Frame: {WorldTime.Instance.Frame}");

            OnEnabled();
        }

        void IEntityFacade.OnDisabled()
        {
            EntityConsole.Instance.Log($"OnDisabled: {Name} Frame: {WorldTime.Instance.Frame}");

            OnDisabled();
        }

        void IEntityFacade.OnDestroyed()
        {
            EntityConsole.Instance.Log($"OnDestroyed: {Name} Frame: {WorldTime.Instance.Frame}");

            OnDestroyed();
        }

        protected virtual void OnRegistered()
        {

        }

        protected virtual void OnUnregistered()
        {

        }

        protected virtual void OnActivated()
        {

        }

        protected virtual void OnDeactivated()
        {

        }

        protected virtual void OnEnabled()
        {

        }

        protected virtual void OnDisabled()
        {

        }

        protected virtual void OnDestroyed()
        {

        }

        #endregion
    }
}