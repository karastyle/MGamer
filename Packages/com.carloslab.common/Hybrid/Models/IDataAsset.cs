// <copyright file="IDataAsset.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.Common
{
    public interface IDataAsset
    {
        string Name { get; }
        int DataVersion { get; }
        bool IsRuntime { get; set; }
        bool IsEditorOpening { get; }
        bool IsRuntimeEditorOpening { get; set; }
        int EditorOpeningCount { get; set; }
        string SerializedModel { get; }
        string FormattedSerializedModel { get; }
        bool BlockRecording { get; }
        bool IsInUndoRedo { get; }
        void Record(string name, Action action, bool save = false);
        void SerializeModel();
        void DeserializeModel();
        void ClearModel();
        void Save();
        void ImportModel(string serializedModel);
    }

    public interface IDataAsset<TRootModel> : IDataAsset
        where TRootModel : class, IRootModel
    {
        TRootModel Model { get; }
    }

    public interface IDataAsset<TRootModel, TRootObject> : IDataAsset<TRootModel>
        where TRootModel : class, IRootModel<TRootObject>
        where TRootObject : class, IRootObject
    {
        TRootObject Runtime { get; }
    }
}