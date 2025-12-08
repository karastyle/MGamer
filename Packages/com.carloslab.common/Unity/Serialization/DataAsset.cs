// <copyright file="DataAsset.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CarlosLab.Common
{
    public abstract class DataAsset<TRootModel, TRootObject>
        : ScriptableObject, IDataAsset<TRootModel, TRootObject>, ISerializationCallbackReceiver
        where TRootModel : class, IRootModel<TRootObject>, new()
        where TRootObject : class, IRootObject
    {
        #region Asset Properties

        private new string name;

        public string Name => name;

        [SerializeField]
        internal int dataVersion = -1;

        public int DataVersion => dataVersion;

        public bool IsInitialized => dataVersion >= 1 && model == null;

        public bool BlockRecording { get; set; }

        public bool IsInUndoRedo { get; set; }

        public TRootObject Runtime => model?.Runtime;

        [NonSerialized]
        private bool isRuntime;

        public bool IsRuntime
        {
            get => model?.IsRuntime ?? isRuntime;
            set
            {
                if (model != null)
                    model.IsRuntime = value;

                isRuntime = value;
            }
        }

        [NonSerialized]
        private int editorOpeningCount;

        public int EditorOpeningCount
        {
            get => model?.EditorOpeningCount ?? editorOpeningCount;
            set
            {
                if (model != null)
                    model.EditorOpeningCount = value;

                editorOpeningCount = value;
            }
        }

        public bool IsEditorOpening => model?.IsEditorOpening ?? false;

        [NonSerialized]
        private bool isRuntimeEditorOpening;

        public bool IsRuntimeEditorOpening
        {
            get => model?.IsRuntimeEditorOpening ?? isRuntimeEditorOpening;
            set
            {
                if (model != null)
                    model.IsRuntimeEditorOpening = value;

                isRuntimeEditorOpening = value;
            }
        }

        [NonSerialized]
        private StreamingContext context;

        protected virtual ref StreamingContext Context
        {
            get
            {
                if (context.Context == null)
                    context = new(StreamingContextStates.All, this);

                return ref context;
            }
        }

        #endregion

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            UpdateFrameworkVersion();
        }

        private void OnEnable()
        {
            this.name = base.name;
        }

        #region Asset Functions

        private void SerializeAndMarkDirty()
        {
            SerializeModel();
            MarkDirty();
        }

        public void Save()
        {
            if (IsRuntime)
                return;

            MarkDirty();
            SaveIfDirty();
        }

        public void Record(string name, Action action, bool save = false)
        {
            if (Application.isPlaying || BlockRecording)
                action?.Invoke();
            else
            {
                CommonConsole.Instance.Log($"{GetType().Name} Record: {name}");
                UndoUtils.RecordObject(this, name);

                action?.Invoke();
                SerializeAndMarkDirty();

                if (save)
                    SaveIfDirty();
            }
        }

        private void MarkDirty()
        {
#if UNITY_EDITOR
            if (IsRuntime)
                return;

            EditorUtility.SetDirty(this);
#endif
        }

        private void SaveIfDirty()
        {
#if UNITY_EDITOR
            if (IsRuntime)
                return;

            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        public void ResetAndSave()
        {
            if (IsRuntime) return;

            UpdateModel();
            Save();
        }

        public void CloseEditor(bool isRuntimeUI)
        {
            if (isRuntimeUI)
                IsRuntimeEditorOpening = false;

            EditorOpeningCount--;
            ResetAndSave();
        }

        public void OpenEditor(bool isRuntimeUI)
        {
            if (isRuntimeUI)
                IsRuntimeEditorOpening = true;

            EditorOpeningCount++;
        }

        #endregion

        #region Model

        [SerializeField]
        protected string serializedModel;

        public string SerializedModel => serializedModel;

        public string FormattedSerializedModel => DataSerializer.Format(serializedModel);


        [NonSerialized]
        private TRootModel model;


        public TRootModel Model
        {
            get
            {
                if (model == null && string.IsNullOrEmpty(serializedModel))
                {
                    model = new TRootModel();
                    UpdateModelVersion(model);
                }
                return model;
            }
        }

        public void ClearModel()
        {
            Record("Clear Model", () =>
            {
                model = new TRootModel();
                UpdateModelInfo(model);
            });
        }

        public void ImportModel(string serializedModel)
        {
            Record("Import Model", () => DeserializeModel(serializedModel));
        }

        private void UpdateModel()
        {
            SerializeModel();
            DeserializeModel();
        }

        public void ResetModel()
        {
            DeserializeModel();
        }

        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            if (Application.isPlaying) return;

            SerializeModel();
        }

        public void OnAfterDeserialize()
        {
            DeserializeModel();
        }

        #endregion

        #region Serialize Model

        public void SerializeModel()
        {
            SerializeModel(model);
        }

        private void SerializeModel(TRootModel model)
        {
            if (model == null) return;

#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("DataAsset - SerializeModel");
#endif

            OnStartSerialization();

            if (DataSerializer.TrySerialize(model, ref Context, out string serializedModel))
            {
                this.serializedModel = serializedModel;
                UpdateFrameworkVersion();
                OnSerializeSuccess();
            }
            else
            {
                OnSerializeFailed();
            }

            OnFinishSerialization();
        }

        protected virtual void OnStartSerialization()
        {

        }

        protected virtual void OnFinishSerialization()
        {

        }

        protected virtual void OnSerializeFailed()
        {

        }

        protected virtual void OnSerializeSuccess()
        {

        }

        protected abstract int GetDataVersion();
        protected abstract string GetFrameworkVersion();

        #endregion

        #region Deserialize Model

        public void DeserializeModel()
        {
            DeserializeModel(serializedModel);
        }

        private void DeserializeModel(string serializedModel)
        {
            if (string.IsNullOrEmpty(serializedModel))
                return;

#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("DataAsset - DeserializeModel");
#endif

            OnStartDeserialization();

            if (DataSerializer.TryDeserialize(serializedModel, ref Context, out TRootModel deserializedModel))
            {
                UpdateModelInfo(deserializedModel);
                this.model = deserializedModel;

                OnDeserializeSuccess();
            }
            else
            {
                OnDeserializeFailed();
            }

            OnFinishDeserialization();
        }

        public bool IsDataVersionValid()
        {
            int dataVersion = GetDataVersion();
            if (this.dataVersion >= 1 && this.dataVersion != dataVersion)
                return false;

            return true;
        }

#if UNITY_EDITOR

        public void ShowDataVersionNotCompatiblePopup()
        {
            int dataVersion = GetDataVersion();
            EditorUtility.DisplayDialog("The data version is not compatible!",
                $"This asset uses data version v{this.dataVersion}, while the framework uses data version v{dataVersion}." +
                $" Please update your asset to data version v{dataVersion} so that the framework can deserialize it.",
                "OK");
        }

#endif

        private void UpdateModelInfo(TRootModel newModel)
        {
            if (newModel == null) return;

            UpdateModelVersion(newModel);

            newModel.IsRuntimeEditorOpening = isRuntimeEditorOpening;
            newModel.EditorOpeningCount = editorOpeningCount;
            newModel.IsRuntime = isRuntime;
        }

        private void UpdateModelVersion(TRootModel newModel)
        {
            if (newModel == null) return;

            newModel.DataVersion = GetDataVersion();
        }

        private void UpdateFrameworkVersion()
        {
            dataVersion = GetDataVersion();
        }

        protected virtual void OnStartDeserialization()
        {
        }

        protected virtual void OnFinishDeserialization()
        {
        }

        protected virtual void OnDeserializeFailed()
        {

        }

        protected virtual void OnDeserializeSuccess()
        {

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