// <copyright file="Composite.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.Extensions;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class Composite : Task
    {
        #region Children Fields

        protected readonly List<Task> children = new();

        public int ChildCount => children.Count;

        public IReadOnlyList<Task> Children => children;

        #endregion

        #region Children Management

        internal void AddChild(Task child)
        {
            AddChild(ChildCount, child);
        }

        internal void AddChild(int index, Task child)
        {
            if (child == null) return;

            child.RootObject = RootObject;
            child.Parent = this;

            children.Insert(index, child);
            OnChildAdded(child);
        }

        protected virtual void OnChildAdded(Task child)
        {
        }

        internal void MoveChild(int sourceIndex, int destIndex)
        {
            children.Move(sourceIndex, destIndex);
            OnChildMoved(sourceIndex, destIndex);
        }

        protected virtual void OnChildMoved(int sourceIndex, int destIndex)
        {
        }

        internal void RemoveChild(Task child)
        {
            int index = children.IndexOf(child);
            if (index < 0)
                return;

            children.RemoveAt(index);
            child.RootObject = null;
            child.Parent = null;
            OnChildRemoved(index);
        }

        protected virtual void OnChildRemoved(int index)
        {
        }

        #endregion

        #region Lifecycle Functions

        internal sealed override void Awake()
        {
            if (Awakened)
                return;

            foreach (Task child in children)
            {
                child.Awake();
            }
            base.Awake();
        }

        protected override void OnAbort()
        {
            AbortChildren();
        }

        protected void AbortChildren()
        {
            foreach (Task child in children)
            {
                child.Abort();
            }
        }

        protected override void OnEnd()
        {
            foreach (Task child in children)
            {
                child.End();
            }
        }

        #endregion

        #region Event Functions

        protected override void OnRootObjectChanged(UtilityIntelligence intelligence)
        {
            foreach (Task child in children)
            {
                child.RootObject = intelligence;
            }
        }

        protected sealed override void OnContextChanged(DecisionContext context)
        {
            foreach (Task child in children)
            {
                child.Context = context;
            }
        }

        #endregion
    }
}