// <copyright file="ConsiderationListViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationListViewModel :
        ContainerViewModel<ConsiderationContainerModel, ConsiderationModel, ConsiderationItemViewModel>
    {
        protected override ConsiderationModel CreateModel(Type runtimeType)
        {
            ConsiderationModel model = base.CreateModel(runtimeType);

            var intelligenceModel = RootViewModel.Model;
            model.InputNormalizationContainer = intelligenceModel.InputNormalizations;

            return model;
        }
    }
}