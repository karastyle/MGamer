// <copyright file="Decision.Context.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision
    {
        #region Context

        private DecisionContext nextContext;

        public DecisionContext NextContext
        {
            get => nextContext;
            internal set
            {
                nextContext = value;

                OnNextContextChanged();
            }
        }

        private void OnNextContextChanged()
        {
            if (!hasNoTarget && context.Target != nextContext.Target)
            {
                Abort();
            }
        }

        private DecisionContext context;

        public DecisionContext Context
        {
            get => context;
            private set
            {
                context = value;
                OnContextChanged(in context);
            }
        }

        private void OnContextChanged(in DecisionContext newContext)
        {
            task.Context = newContext;
        }

        private void ClearContexts()
        {
            context = DecisionContext.Null;
            nextContext = DecisionContext.Null;
        }

        #endregion
    }
}