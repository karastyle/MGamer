// <copyright file="UtilityWorldController.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    [AddComponentMenu(FrameworkConsts.AddWorldControllerMenuPath)]
    public class UtilityWorldController : WorldController<UtilityWorld>
    {
        [SerializeField]
        [FormerlySerializedAs("makeDecisionInterval")]
        private float decisionMakingInterval = 0.1f;

        [SerializeField]
        private bool enableDecisionMakingBatchProcessing = false;

        [SerializeField]
        private int decisionMakingBatchSize = 40;

        protected override UtilityWorld CreateWorld()
        {
            return new UtilityWorld()
            {
                DecisionMakingInterval = decisionMakingInterval,
                EnableDecisionMakingBatchProcessing = enableDecisionMakingBatchProcessing,
                DecisionMakingBatchSize = decisionMakingBatchSize
            };
        }

        public UtilityEntity GetEntity(int id)
        {
            return World?.GetEntity(id);
        }
    }
}