// <copyright file="DecisionMakerContext.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public struct DecisionMakerContext : IContext, IEquatable<DecisionMakerContext>
    {
        public static readonly DecisionMakerContext Null = new();
        public DecisionMakerContext(DecisionMaker decisionMaker) : this()
        {
            DecisionMaker = decisionMaker;
        }

        #region DecisionMaker

        public readonly DecisionMaker DecisionMaker;

        public string DecisionMakerName => DecisionMaker?.Name;

        public UtilityIntelligence Intelligence => DecisionMaker?.Intelligence;

        #endregion

        #region Result

        public float Score;
        public bool IsWinner;

        public Decision BestDecision;

        public string BestDecisionName => BestDecision?.Name;

        #endregion

        #region DecisionContexts

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private Dictionary<string, DecisionContext> decisionContexts;

        private Dictionary<string, DecisionContext> DecisionContexts
        {
            get
            {
                if (DecisionMaker == null) return null;

                if (decisionContexts == null
                    && Intelligence.IsEditorOpening
                   )
                    decisionContexts = new(10);
                return decisionContexts;
            }
        }

        public DecisionContext GetDecisionContext(string name)
        {
            if (DecisionContexts != null && DecisionContexts.TryGetValue(name, out DecisionContext context))
                return context;
            return DecisionContext.Null;
        }

        public void AddDecisionContext(string name, in DecisionContext decisionContext)
        {
            if (DecisionContexts == null) return;

            DecisionContexts.TryAdd(name, decisionContext);
        }

        public void SetDecisionContext(string name, in DecisionContext decisionContext)
        {
            if (DecisionContexts == null) return;

            DecisionContexts[name] = decisionContext;
        }
#endif
        #endregion

        #region IEquatable

        public bool Equals(DecisionMakerContext other)
        {
            return DecisionMaker == other.DecisionMaker
                && BestDecision == other.BestDecision;
        }

        // public bool EqualsValue(DecisionMakerContext other)
        // {
        //     return Equals(other) 
        //            && Math.Abs(Score - other.Score) < MathUtils.Epsilon;
        // }

        public override bool Equals(object obj)
        {
            return obj is DecisionMakerContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DecisionMaker, BestDecision);
        }

        public static bool operator ==(DecisionMakerContext left, DecisionMakerContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DecisionMakerContext left, DecisionMakerContext right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}