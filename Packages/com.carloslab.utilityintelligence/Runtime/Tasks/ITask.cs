// <copyright file="ITask.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public interface ITask
    {
        ExecuteStatus Execute(float deltaTime);
        internal void Abort();
        internal void Reset();
    }
}