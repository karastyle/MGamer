// <copyright file="DecisionNameItemViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionNameItemViewIntelligenceTab : WinnerStatusNameItemView<DecisionItemViewModelIntelligenceTab>
    {
        public DecisionNameItemViewIntelligenceTab() : base(true, false, true)
        {
        }

        protected override void OnRegisterViewModelEvents(DecisionItemViewModelIntelligenceTab viewModel)
        {
            base.OnRegisterViewModelEvents(viewModel);

            OnDecisionContextChanged(viewModel.ContextViewModel.Context);
            viewModel.ContextChanged += OnDecisionContextChanged;
        }

        protected override void OnUnregisterViewModelEvents(DecisionItemViewModelIntelligenceTab viewModel)
        {
            base.OnUnregisterViewModelEvents(viewModel);

            viewModel.ContextChanged -= OnDecisionContextChanged;
        }

        private void OnDecisionContextChanged(DecisionContext context)
        {
            if (!IsRuntime)
                IsWinner = context.IsWinner;
        }
    }
}