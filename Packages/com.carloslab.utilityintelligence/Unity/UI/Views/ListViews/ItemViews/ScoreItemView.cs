// <copyright file="ScoreItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ScoreItemView<TItemViewModel> : BaseNameItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        private float score;

        public ScoreItemView(bool enableRemove = true) : base(false, enableRemove)
        {
            NameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        }

        [CreateProperty]
        public float Score
        {
            get => score;
            set
            {
                if (Math.Abs(score - value) < MathUtils.Epsilon)
                    return;

                score = value;
                UpdateScoreLabel(score);
            }
        }

        protected void UpdateScoreLabel(float score)
        {
            NameLabel.text = score.ToString("F3");
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            UpdateScoreLabel(Score);
            SetBinding(nameof(Score), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged,
                dataSourcePath = PropertyPath.FromName(nameof(IScoreViewModel.Score))
            });
        }

        protected override void OnResetView()
        {
            ClearBindings();
        }
    }
}