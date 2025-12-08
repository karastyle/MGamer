// <copyright file="IImapWorld.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IImapWorld
    {
        IImapManager MapManager { get; }
        IImapSpaceHandler MapSpaceHandler { get; }
    }
}