// <copyright file="UtilityIntelligenceContext.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public struct UtilityIntelligenceContext : IContext, IEquatable<UtilityIntelligenceContext>
    {
        public static readonly UtilityIntelligenceContext Null = new();

        public UtilityIntelligenceContext(UtilityIntelligence intelligence) : this()
        {
            Intelligence = intelligence;
        }

        #region UtilityIntelligence

        public readonly UtilityIntelligence Intelligence;

        public DecisionMaker BestDecisionMaker;
        public Decision BestDecision;

        public float Score;

        #endregion

        #region DecisionMakerContexts

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private Dictionary<string, DecisionMakerContext> decisionMakerContexts;

        private Dictionary<string, DecisionMakerContext> DecisionMakerContexts
        {
            get
            {
                if (Intelligence == null) return null;

                if (decisionMakerContexts == null
                    && Intelligence.IsEditorOpening
                   )
                    decisionMakerContexts = new(5);
                return decisionMakerContexts;
            }
        }

        public DecisionMakerContext GetDecisionMakerContext(string name)
        {
            if (DecisionMakerContexts != null && DecisionMakerContexts.TryGetValue(name, out DecisionMakerContext context))
                return context;
            return DecisionMakerContext.Null;
        }

        public void AddDecisionMakerContext(string name, in DecisionMakerContext decisionMakerContext)
        {
            if (DecisionMakerContexts == null) return;

            DecisionMakerContexts.TryAdd(name, decisionMakerContext);
        }

        public void SetDecisionMakerContext(string name, in DecisionMakerContext decisionMakerContext)
        {
            if (DecisionMakerContexts == null) return;

            DecisionMakerContexts[name] = decisionMakerContext;
        }
#endif

        #endregion

        #region IEquatable

        public bool Equals(UtilityIntelligenceContext other)
        {
            return Intelligence == other.Intelligence
                    && BestDecisionMaker == other.BestDecisionMaker
                    && BestDecision == other.BestDecision;
        }

        // public bool EqualsValue(UtilityIntelligenceContext other)
        // {
        //     return Equals(other) 
        //            && Math.Abs(Score - other.Score) < MathUtils.Epsilon;
        // }

        public override bool Equals(object obj)
        {
            return obj is UtilityIntelligenceContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Intelligence, BestDecisionMaker, BestDecision);
        }

        public static bool operator ==(UtilityIntelligenceContext left, UtilityIntelligenceContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UtilityIntelligenceContext left, UtilityIntelligenceContext right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}