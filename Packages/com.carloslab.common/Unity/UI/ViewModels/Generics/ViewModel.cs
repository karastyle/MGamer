// <copyright file="ViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class ViewModel<TModel> : IViewModel<TModel>
        where TModel : class, IModel
    {
        private string id;
        public string Id => id;

        public ViewModel()
        {
            id = Guid.NewGuid().ToString();
        }

        #region Model

        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public void Init(TModel model)
        {
            if (isInitialized || model == null) return;

            this.model = model;
            OnInit(model);

            RegisterModelEvents(model);
            isInitialized = true;
        }

        public void Deinit()
        {
            if (!isInitialized) return;
            UnregisterModelEvents(model);

            OnDeinit();

            this.model = null;
            isInitialized = false;
        }

        protected virtual void OnDeinit()
        {

        }

        protected virtual void OnInit(TModel model)
        {

        }

        public event Action<IModel> ModelChanged;

        public object ModelObject => model;

        private TModel model;

        public TModel Model
        {
            get => model;
            set
            {
                if (model == value) return;

                UnregisterModelEvents(model);

                model = value;

                RegisterModelEvents(model);

                HandleModelChanged(model);
            }
        }

        public void ClearModel()
        {
            Model = null;
        }

        protected void RaiseModelChanged(IModel newModel)
        {
            ModelChanged?.Invoke(newModel);
        }

        private void HandleModelChanged(TModel newModel)
        {
            OnModelChanged(newModel);
            RaiseModelChanged(newModel);
        }

        private void RegisterModelEvents(TModel model)
        {
            if (model != null)
                OnRegisterModelEvents(model);
        }

        private void UnregisterModelEvents(TModel model)
        {
            if (model != null)
                OnUnregisterModelEvents(model);
        }

        protected virtual void OnModelChanged(TModel newModel)
        {
        }

        protected virtual void OnRegisterModelEvents(TModel model)
        {
        }

        protected virtual void OnUnregisterModelEvents(TModel model)
        {
        }

        #endregion

        #region IDataSourceViewHashProvider

        public event Action ViewHashCodeChanged;


        private long viewHashCode;

        public void UpdateViewHashCode()
        {
            viewHashCode++;
            ViewHashCodeChanged?.Invoke();
        }

        public long GetViewHashCode()
        {
            return viewHashCode;
        }

        #endregion

        #region INotifyBindablePropertyChanged

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;


        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        #endregion
    }
}