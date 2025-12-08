// <copyright file="DecisionContext.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public struct DecisionContext : IContext, IEquatable<DecisionContext>
    {
        public static readonly DecisionContext Null = new();

        public DecisionContext(Decision decision) : this()
        {
            Decision = decision;
        }

        public DecisionContext(Decision decision, UtilityEntity target, DecisionMaker decisionMaker) : this(decision)
        {
            Target = target;
            DecisionMaker = decisionMaker;
        }

        #region Decision

        public readonly Decision Decision;

        public string DecisionName => Decision?.Name;

        public UtilityIntelligence Intelligence => Decision?.Intelligence;

        #endregion

        #region Context

        public readonly UtilityEntity Target;

        public IEntityFacade TargetFacade => Target?.EntityFacade;

        public readonly DecisionMaker DecisionMaker;

        public string DecisionMakerName => DecisionMaker?.Name;

        #endregion

        #region Result

        public float Score;

        public float FinalScore;

        public bool IsWinner;

        public bool Discarded;

        public void SetResult(in DecisionResult result)
        {
            Discarded = result.Discarded;
            Score = result.Score;
        }

        #endregion

        #region ConsiderationContexts

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private Dictionary<string, ConsiderationContext> considerationContexts;

        private Dictionary<string, ConsiderationContext> ConsiderationContexts
        {
            get
            {

                if (Decision == null) return null;

                if (considerationContexts == null
                    && Intelligence.IsEditorOpening
                   )
                    considerationContexts = new(10);
                return considerationContexts;
            }
        }

        public ConsiderationContext GetConsiderationContext(string name)
        {
            if (ConsiderationContexts != null && ConsiderationContexts.TryGetValue(name, out ConsiderationContext context))
                return context;
            return ConsiderationContext.Null;
        }

        public void AddConsiderationContext(string name, in ConsiderationContext considerationContext)
        {
            if (ConsiderationContexts == null) return;

            ConsiderationContexts.TryAdd(name, considerationContext);
        }
#endif
        #endregion

        #region IEquatable

        public bool Equals(DecisionContext other)
        {
            return Decision == other.Decision
                   && Target == other.Target
                   && DecisionMaker == other.DecisionMaker;
        }

        // public bool EqualsValue(DecisionContext other)
        // {
        //     return Equals(other) 
        //            && Math.Abs(Score - other.Score) < MathUtils.Epsilon;
        // }

        public override bool Equals(object obj)
        {
            return obj is DecisionContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Decision, Target, DecisionMaker);
        }

        public static bool operator ==(DecisionContext left, DecisionContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DecisionContext left, DecisionContext right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}