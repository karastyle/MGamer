// <copyright file="ActionItemViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionItemViewModel : BaseItemViewModel<ActionModel, ActionListViewModel>, ITypeNameViewModel
    {
        public string TypeName => Model?.RuntimeType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;
    }
}