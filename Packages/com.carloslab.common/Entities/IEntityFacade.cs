// <copyright file="IEntityFacade.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IEntityFacade : IEntityHandler
    {
        IEntity Entity { get; internal set; }
        Float3 Position { get; }

        internal void OnRegistered();
        internal void OnUnregistered();
        internal void OnActivated();
        internal void OnDeactivated();
        internal void OnEnabled();
        internal void OnDisabled();

        internal void OnDestroyed();

        internal void ActivateInternal();
        internal void DeactivateInternal();
        internal void DestroyInternal();
    }
}