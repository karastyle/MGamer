// <copyright file="UtilityIntelligenceAsset.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    [CreateAssetMenu(menuName = FrameworkConsts.CreateAssetMenuPath, fileName = FrameworkConsts.AssetFileName)]
    public class UtilityIntelligenceAsset : DataAsset<UtilityIntelligenceModel, UtilityIntelligence>
    {
        [SerializeField]
        [FormerlySerializedAs("agentType")]
        internal string type;

        [SerializeField]
        [FormerlySerializedAs("agentDescription")]
        internal string description;

        protected override int GetDataVersion()
        {
            return FrameworkConsts.DataVersion;
        }

        protected override string GetFrameworkVersion()
        {
            return FrameworkConsts.FrameworkVersion;
        }
    }
}