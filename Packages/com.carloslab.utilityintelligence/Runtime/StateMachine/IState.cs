// <copyright file="IState.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public interface IState
    {
        string Name { get; }
        bool CanGoToNextState { get; }
        internal void Enter();
        internal void Exit();
        internal void Reset();
    }
}