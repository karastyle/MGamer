// <copyright file="DecisionModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Extensions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class DecisionModel : ContainerItemModel<DecisionContainerModel, Decision>
    {
        #region Decision

        [DataMember(Name = nameof(Category))]
        private string category;

        public string Category
        {
            get => category;
            internal set
            {
                category = value;
                Runtime.Category = value;
            }
        }

        [DataMember(Name = nameof(Weight))]
        private float weight = 1.0f;
        public float Weight
        {
            get => weight;
            set
            {
                weight = value;
                Runtime.Weight = value;
            }
        }

        [DataMember(Name = nameof(HasNoTarget))]
        private bool hasNoTarget;
        public bool HasNoTarget
        {
            get => hasNoTarget;
            set
            {
                hasNoTarget = value;
                Runtime.HasNoTarget = value;
            }
        }

        [DataMember(Name = nameof(EnableCachePerTarget))]
        private bool enableCachePerTarget;
        public bool EnableCachePerTarget
        {
            get => enableCachePerTarget;
            set
            {
                enableCachePerTarget = value;
                Runtime.EnableCachePerTarget = value;
            }
        }

        [DataMember(Name = nameof(KeepRunningUntilFinished))]
        private bool keepRunningUntilFinished;
        public bool KeepRunningUntilFinished
        {
            get => keepRunningUntilFinished;
            set
            {
                keepRunningUntilFinished = value;
                Runtime.KeepRunningUntilFinished = value;
            }
        }

        [DataMember(Name = nameof(MaxRepeatCount))]
        private int maxRepeatCount;
        public int MaxRepeatCount
        {
            get => maxRepeatCount;
            set
            {
                maxRepeatCount = value;
                Runtime.Task.MaxRepeatCount = value;
            }
        }

        public int CurrentRepeatCount
        {
            get
            {
                if (Runtime.Task != null) return Runtime.Task.CurrentRepeatCount;

                return 0;
            }
        }

        #endregion

        #region Target Filters

        private TargetFilterContainerModel targetFilterContainer;

        public TargetFilterContainerModel TargetFilterContainer
        {
            get => targetFilterContainer;
            internal set => targetFilterContainer = value;
        }

        [DataMember(Name = nameof(TargetFilters))]
        private List<string> targetFilterNames = new();

        private List<TargetFilterModel> targetFilters;

        public List<TargetFilterModel> TargetFilters
        {
            get
            {
                if (targetFilters == null)
                {
                    targetFilters = new();
                    if (targetFilterContainer != null)
                    {
                        foreach (string targetFilterName in targetFilterNames)
                        {
                            if (targetFilterContainer.TryGetItem(targetFilterName, out TargetFilterModel targetFilter))
                                targetFilters.Add(targetFilter);
                            else
                                StaticConsole.LogWarning($"Asset: {Asset?.Name} Decision: {Name} Cannot find the TargetFilter: {targetFilterName}." +
                                                         $" Please remove it from your JSON using File Menu Toolbar.");
                        }
                    }
                }

                return targetFilters;
            }
        }

        public bool HasTargetFilter(string name)
        {
            return Runtime.HasTargetFilter(name);
        }

        public bool TryAddTargetFilter(int index, string name, TargetFilterModel targetFilter)
        {
            if (Runtime.TryAddTargetFilter(index, name, targetFilter.Runtime))
            {
                targetFilterNames.Insert(index, targetFilter.Name);
                targetFilters?.Insert(index, targetFilter);
                return true;
            }

            return false;
        }

        public TargetFilterModel GetTargetFilterById(string id)
        {
            return targetFilters.Find(model => model.Id == id);
        }

        public bool TryRemoveTargetFilter(string name, TargetFilterModel model)
        {
            if (Runtime.TryRemoveTargetFilter(name))
            {
                targetFilterNames.Remove(name);
                targetFilters?.Remove(model);
                return true;
            }

            return false;
        }

        public bool TryChangeTargetFilterName(string oldName, string newName)
        {
            int index = targetFilterNames.IndexOf(oldName);
            if (index >= 0)
            {
                targetFilterNames[index] = newName;
                return true;
            }

            return false;
        }

        public void MoveTargetFilter(int sourceIndex, int destIndex)
        {
            targetFilterNames.Move(sourceIndex, destIndex);
            targetFilters?.Move(sourceIndex, destIndex);
            Runtime.MoveTargetFilter(sourceIndex, destIndex);

        }

        #endregion

        #region Action Tasks

        [DataMember(Name = nameof(Actions))]
        private List<ActionModel> actions = new();

        public IReadOnlyList<ActionModel> Actions => actions;

        [DataMember(Name = nameof(ActionExecutionMode))]
        private ActionExecutionMode actionExecutionMode = ActionExecutionMode.Sequence;

        public ActionExecutionMode ActionExecutionMode => actionExecutionMode;

        public void SetActionsExecutionMode(ActionExecutionMode newMode, bool awakeTask)
        {
            if (actionExecutionMode == newMode) return;

            actionExecutionMode = newMode;
            var composite = CreateCompositeTask();
            composite.RootObject = Runtime.RootObject;
            Runtime.Task.Child = composite;
            if (awakeTask) composite.Awake();
        }

        public void AddAction(ActionModel action, bool awakeAction)
        {
            actions.Add(action);
            if (Runtime.Task.Child is Composite compositeTask)
            {
                compositeTask.AddChild(action.Runtime);
                if (awakeAction) action.Runtime.Awake();
            }
        }

        public void AddAction(int index, ActionModel action, bool awakeAction)
        {
            actions.Insert(index, action);

            var runtimeAction = action.Runtime;
            runtimeActions.Insert(index, runtimeAction);

            if (Runtime.Task.Child is Composite compositeTask)
            {
                compositeTask.AddChild(index, runtimeAction);
                if (awakeAction) runtimeAction.Awake();
            }
        }

        public ActionModel GetActionById(string id)
        {
            return actions.Find(action => action.Id == id);
        }

        public void MoveAction(int sourceIndex, int destIndex)
        {
            actions.Move(sourceIndex, destIndex);
            runtimeActions.Move(sourceIndex, destIndex);
            if (Runtime.Task.Child is Composite compositeTask)
                compositeTask.MoveChild(sourceIndex, destIndex);
        }

        public void RemoveAction(ActionModel action)
        {
            actions.Remove(action);

            var runtimeAction = action.Runtime;
            runtimeActions.Remove(runtimeAction);
            if (Runtime.Task.Child is Composite compositeTask)
                compositeTask.RemoveChild(runtimeAction);
        }

        public void SetVariableReference(string oldVariableName, string newVariableName)
        {
            foreach (var action in actions)
            {
                action.SetVariableReference(oldVariableName, newVariableName);
            }
        }

        #endregion

        #region Considerations

        private ConsiderationContainerModel considerationContainer;

        public ConsiderationContainerModel ConsiderationContainer
        {
            get => considerationContainer;
            internal set => considerationContainer = value;
        }

        [DataMember(Name = nameof(Considerations))]
        private List<string> considerationNames = new();

        private List<ConsiderationModel> considerations;
        public List<ConsiderationModel> Considerations
        {
            get
            {
                if (considerations == null)
                {
                    considerations = new List<ConsiderationModel>();

                    if (considerationContainer != null)
                    {
                        foreach (string considerationName in considerationNames)
                        {
                            if (considerationContainer.TryGetItem(considerationName, out ConsiderationModel consideration))
                                considerations.Add(consideration);
                            else
                                StaticConsole.LogWarning($"Asset: {Asset?.Name} Decision: {Name} Cannot find the Consideration: {considerationName}." +
                                                 $" Please remove it from your JSON using File Menu Toolbar.");
                        }
                    }
                }

                return considerations;
            }
        }

        public bool HasConsideration(string name)
        {
            return Runtime.HasConsideration(name);
        }

        public bool TryAddConsideration(int index, string name, ConsiderationModel consideration)
        {
            if (Runtime.TryAddConsideration(index, name, consideration.Runtime))
            {
                considerationNames.Insert(index, name);
                considerations?.Insert(index, consideration);
                return true;
            }

            return false;
        }

        public ConsiderationModel GetConsiderationById(string id)
        {
            return considerations.Find(consideration => consideration.Id == id);
        }

        public bool TryRemoveConsideration(string name, ConsiderationModel consideration)
        {
            if (Runtime.TryRemoveConsideration(name))
            {
                considerationNames.Remove(name);
                considerations?.Remove(consideration);
                return true;
            }

            return false;
        }

        public bool TryChangeConsiderationName(string oldName, string newName)
        {
            int index = considerationNames.IndexOf(oldName);
            if (index >= 0)
            {
                considerationNames[index] = newName;
                return true;
            }

            return false;
        }

        public void MoveConsideration(int sourceIndex, int destIndex)
        {
            considerationNames.Move(sourceIndex, destIndex);
            considerations?.Move(sourceIndex, destIndex);
            Runtime.MoveConsideration(sourceIndex, destIndex);
        }

        #endregion

        #region Runtime

        private List<ActionTask> runtimeActions;
        public List<ActionTask> RuntimeActions => runtimeActions;

        private Decision runtime;

        public override Decision Runtime
        {
            get
            {
                if (runtime == null)
                {
                    Repeater task = CreateRepeaterTask();
                    runtime = new Decision(task)
                    {
                        Weight = weight,
                        KeepRunningUntilFinished = keepRunningUntilFinished,
                        HasNoTarget = hasNoTarget,
                        EnableCachePerTarget = enableCachePerTarget
                    };

                    for (int index = 0; index < Considerations.Count; index++)
                    {
                        ConsiderationModel consideration = Considerations[index];
                        runtime.TryAddConsiderationWithoutCompensation(consideration.Name, consideration.Runtime);
                    }

                    for (int index = 0; index < TargetFilters.Count; index++)
                    {
                        TargetFilterModel targetFilter = TargetFilters[index];
                        runtime.TryAddTargetFilter(targetFilter.Name, targetFilter.Runtime);
                    }

                    runtime.Init();
                }

                return runtime;
            }
        }

        private Composite CreateCompositeTask()
        {
            Composite composite = null;
            switch (actionExecutionMode)
            {
                case ActionExecutionMode.Sequence:
                    composite = new Sequencer();
                    break;
                case ActionExecutionMode.Parallel:
                    composite = new Parallel();
                    break;
                case ActionExecutionMode.ParallelComplete:
                    composite = new ParallelComplete();
                    break;
            }

            runtimeActions = new();

            foreach (ActionModel action in actions)
            {
                var runtimeAction = action.Runtime;
                runtimeActions.Add(runtimeAction);
                composite.AddChild(runtimeAction);
            }

            return composite;
        }

        private Repeater CreateRepeaterTask()
        {
            Task childTask = CreateCompositeTask();

            Repeater repeater = new()
            {
                Child = childTask,
                MaxRepeatCount = this.maxRepeatCount,
            };

            return repeater;
        }

        #endregion

    }
}