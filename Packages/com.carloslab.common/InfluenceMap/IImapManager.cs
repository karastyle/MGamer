// <copyright file="IImapManager.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IImapManager
    {
        void AddInfluence(int layerId, int mapType, Int2 cellIndexWorld, int radius, float magnitude);
    }
}