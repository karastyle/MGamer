// <copyright file="UtilityIntelligenceViewModelMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceViewModelMember<TModel> : RootViewModelMember<TModel, UtilityIntelligenceViewModel>
        where TModel : class, IModel
    {

    }
}