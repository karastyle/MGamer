// <copyright file="IImapEntity.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IImapEntity : IEntity

    {
        IImapWorld MapWorld { get; }
        IImapSpaceHandler MapSpaceHandler { get; }
        IImapManager MapManager { get; }

        float GetInfluence(int layerId, int mapType);
    }
}