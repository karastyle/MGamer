// <copyright file="ConsiderationScoreItemViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationScoreItemViewIntelligenceTab : ScoreItemView<ConsiderationItemViewModelIntelligenceTab>
    {
        public ConsiderationScoreItemViewIntelligenceTab() : base(false)
        {
        }

        protected override void OnRefreshView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            UpdateScoreLabel(Score);
            SetBinding(nameof(Score), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged,
                dataSourcePath = PropertyPath.FromName(nameof(ConsiderationContextViewModel.Score))
            });
        }
    }
}