// <copyright file="InputListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputListViewModel : ContainerViewModel<InputContainerModel, InputModel, InputItemViewModel>
    {
        protected override InputModel CreateModel(Type runtimeType)
        {
            var blackboard = Model.Runtime.Blackboard;
            var model = GenericModelFactory.CreateWithId<InputModel>(runtimeType, blackboard);

            return model;
        }
    }
}