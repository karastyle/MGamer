// <copyright file="DecisionMakerListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        DecisionMakerListViewModel : ContainerViewModel<DecisionMakerContainerModel, DecisionMakerModel, DecisionMakerItemViewModel>
    {

        protected override DecisionMakerModel CreateModel(Type runtimeType)
        {
            var model = base.CreateModel(runtimeType);

            var intelligenceModel = RootViewModel.Model;
            model.DecisionContainer = intelligenceModel.Decisions;

            return model;
        }
    }
}