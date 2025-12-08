// <copyright file="DecisionListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionListViewModel : ContainerViewModel<DecisionContainerModel, DecisionModel, DecisionItemViewModel>
    {
        protected override DecisionModel CreateModel(Type runtimeType)
        {
            DecisionModel model = base.CreateModel(runtimeType);

            var intelligenceModel = RootViewModel.Model;

            model.ConsiderationContainer = intelligenceModel.Considerations;
            model.TargetFilterContainer = intelligenceModel.TargetFilters;

            return model;
        }
    }
}