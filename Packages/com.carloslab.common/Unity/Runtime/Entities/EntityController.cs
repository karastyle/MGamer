// <copyright file="EntityController.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections;
using UnityEngine;

namespace CarlosLab.Common
{
    [DefaultExecutionOrder(-1000)]
    public abstract class EntityController<TEntity> : MonoBehaviour, IEntityController
        where TEntity : class, IEntity
    {
        #region Event Functions
        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            entity?.OnActivated();
        }

        private void OnDisable()
        {
            entity?.OnDeactivated();
        }

        private void OnDestroy()
        {
            entity?.OnDestroyed();
        }

        #endregion

        #region Init

        protected virtual void Init()
        {
            if (entity != null)
                return;

            entity = CreateEntity();
            entity.Name = name;
            IEntityFacade entityFacade = GetComponent<IEntityFacade>();
            entityFacade.Entity = entity;
            entity.EntityFacade = entityFacade;
            OnInit();
        }

        protected virtual void OnInit()
        {
        }

        #endregion

        #region Entity Properties

        internal TEntity entity;
        public int Id => entity?.Id ?? -1;

        public bool IsRegistered => entity is { IsRegistered: true };
        public bool IsActive => entity is { IsActive: true };
        public bool IsEnabled => entity is { IsEnabled: true };
        public bool IsDestroyed => entity is { IsDestroyed: true };

        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (entity != null) entity.Name = name;
            }
        }

        public TEntity Entity => entity;

        #endregion

        #region Entity Methods

        protected abstract TEntity CreateEntity();

        public void SetActive(bool active)
        {
            entity?.SetActive(active);
        }

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

        public void Activate()
        {
            entity?.Activate();
        }

        public void Deactivate()
        {
            entity?.Deactivate();
        }

        public void SetEnable(bool enable)
        {
            entity?.SetEnable(enable);
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

        public void Destroy()
        {
            entity?.Destroy();
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

        #endregion

    }

    public abstract class EntityController<TEntity, TDataAsset> : EntityController<TEntity>
        where TEntity : class, IEntity
        where TDataAsset : ScriptableObject, IDataAsset
    {
        [SerializeField]
        internal TDataAsset editorAsset;

        internal TDataAsset runtimeAsset;

        public TDataAsset EditorAsset => editorAsset;

        public TDataAsset RuntimeAsset => runtimeAsset;

        public TDataAsset Asset
        {
            get
            {
                if (Application.isPlaying)
                    return runtimeAsset;
                return editorAsset;
            }
        }

        protected sealed override void Init()
        {
            InitRuntime();
            base.Init();
        }

        private void InitRuntime()
        {
            if (editorAsset == null)
            {
                StaticConsole.LogError($"{name} contains null DataAsset");
                return;
            }

            if (runtimeAsset != null)
                return;

            runtimeAsset = Instantiate(editorAsset);
            runtimeAsset.IsRuntime = true;
        }
    }
}