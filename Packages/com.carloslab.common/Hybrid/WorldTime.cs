// <copyright file="WorldTime.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IWorldTime
    {
        int Frame { get; }
        float Time { get; }
    }
    public partial class WorldTime : Singleton<WorldTime>, IWorldTime
    {

    }
}