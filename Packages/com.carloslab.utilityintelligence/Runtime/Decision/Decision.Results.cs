// <copyright file="Decision.Results.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public partial class Decision
    {
        private DecisionResult noTargetResult;
        private Dictionary<int, DecisionResult> targetResults = new(20);

        internal void Reset()
        {
            // score = 0.0f;
            noTargetResult = DecisionResult.Null;
            targetResults.Clear();
        }

        private bool TryGetResult(int targetId, out DecisionResult result)
        {
            if (targetId < 0)
            {
                result = DecisionResult.Null;
                return false;
            }

            return targetResults.TryGetValue(targetId, out result);
        }

        private bool TryAddResult(int targetId, DecisionResult result)
        {
            if (targetId < 0) return false;

            if (targetResults.TryAdd(targetId, result))
                return true;

            return false;
        }
    }
}