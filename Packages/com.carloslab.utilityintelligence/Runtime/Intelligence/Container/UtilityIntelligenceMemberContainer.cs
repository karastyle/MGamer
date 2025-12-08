// <copyright file="UtilityIntelligenceMemberContainer.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityIntelligenceMemberContainer<TItem> : ItemContainer<TItem>, IUtilityIntelligenceMember
        where TItem : class, IUtilityIntelligenceMember, IContainerItem
    {

        private UtilityIntelligence rootObject;

        public UtilityIntelligence RootObject
        {
            get => rootObject;
            set
            {
                if (rootObject == value) return;

                rootObject = value;

                OnRootObjectChanged(rootObject);
            }
        }

        public bool IsEditorOpening => rootObject?.IsEditorOpening ?? false;
        public bool IsRuntime => rootObject?.IsRuntime ?? false;

        public UtilityIntelligence Intelligence => rootObject;
        public Blackboard Blackboard => rootObject.Blackboard;
        public UtilityAgent Agent => rootObject.Agent;
        public IEntityFacade AgentFacade => Agent.EntityFacade;

        public T GetComponent<T>()
        {
            return Agent.GetComponent<T>();
        }

        public T GetComponentInChildren<T>()
        {
            return Agent.GetComponentInChildren<T>();
        }

        private void OnRootObjectChanged(UtilityIntelligence rootObject)
        {
            foreach (var item in items)
            {
                item.RootObject = rootObject;
            }
        }

        protected override void OnItemAdded(TItem item)
        {
            item.RootObject = rootObject;
        }

        protected override void OnItemRemoved(TItem item)
        {
            item.RootObject = null;
        }
    }
}