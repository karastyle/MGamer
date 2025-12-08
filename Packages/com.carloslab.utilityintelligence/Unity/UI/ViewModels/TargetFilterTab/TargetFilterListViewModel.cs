// <copyright file="TargetFilterListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewModel :
        ContainerViewModel<TargetFilterContainerModel, TargetFilterModel, TargetFilterItemViewModel>
    {
        protected override TargetFilterModel CreateModel(Type runtimeType)
        {
            var blackboard = Model.Runtime.Blackboard;
            var model = GenericModelFactory.CreateWithId<TargetFilterModel>(runtimeType, blackboard);
            return model;
        }
    }
}