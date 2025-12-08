// <copyright file="ActionListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionListViewModel : BaseListViewModel<DecisionModel, ActionModel, ActionItemViewModel>
    {
        public override IReadOnlyList<ActionModel> ItemModels => Model.Actions;

        protected override ActionModel CreateModel(Type runtimeType)
        {
            var blackboard = Model.Runtime.Blackboard;
            var model = GenericModelFactory.CreateWithId<ActionModel>(runtimeType, blackboard);
            return model;
        }

        protected override bool TryAddModelWithoutRecord(ActionModel model, int index)
        {
            Model.AddAction(index, model, IsRuntime);
            return true;
        }

        protected override bool TryRemoveModelWithoutRecord(ActionModel model)
        {
            Model.RemoveAction(model);
            return true;
        }

        public override bool TryRenameItem(ActionItemViewModel item, string newName)
        {
            return false;
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveAction(sourceIndex, destIndex);
        }
    }
}