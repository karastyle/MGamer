// <copyright file="Decorator.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence
{
    public abstract class Decorator : Task
    {
        protected Task child;

        public Task Child
        {
            get => child;
            internal set
            {
                if (child == value) return;

                if (child != null)
                {
                    child.RootObject = null;
                    child.Parent = null;
                }

                child = value;

                if (child != null)
                {
                    child.RootObject = RootObject;
                    child.Parent = this;
                }
            }
        }

        #region Lifecycle Functions

        internal sealed override void Awake()
        {
            if (Awakened)
                return;

            child?.Awake();
            base.Awake();
        }

        protected override void OnAbort()
        {
            child?.Abort();
        }

        protected override void OnEnd()
        {
            child?.End();
        }

        #endregion


        #region Event Functions

        protected override void OnRootObjectChanged(UtilityIntelligence intelligence)
        {
            if (child != null)
                child.RootObject = intelligence;
        }

        protected sealed override void OnContextChanged(DecisionContext context)
        {
            if (child != null)
                child.Context = context;
        }

        #endregion
    }
}