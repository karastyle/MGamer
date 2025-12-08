// <copyright file="IImapSpaceHandler.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common
{
    public interface IImapSpaceHandler
    {
        Int2 WorldPosToCellIndexWorld(Float2 worldPos);
        Int2 WorldPosToMapIndex(Float2 worldPos);
        Int2 WorldPosToMapCellIndexWorld(Float2 worldPos);
        Int2 WorldPosToCellIndexMap(Float2 worldPos, Int2 mapIndex);
        Float2 CellIndexWorldToWorldPos(Int2 worldIndex);
        Int2 CellIndexWorldToMapIndex(Int2 cellIndexWorld);
        Int2 CellIndexWorldToCellIndexMap(Int2 cellIndexWorld, Int2 mapIndex);
        Int2 MapIndexToCellIndexWorld(Int2 mapIndex);
        Float2 CellIndexMapToWorldPos(Int2 cellMapIndex, Int2 mapIndex);
        Int2 CellIndexMapToCellIndexWorld(Int2 cellIndexMap, Int2 mapIndex);
        bool CellIndexWorldInBounds(Int2 cellIndexWorld);
        bool WorldPosInBounds(Float2 worldPos);
        bool MapIndexInBounds(Int2 mapIndex);
    }
}