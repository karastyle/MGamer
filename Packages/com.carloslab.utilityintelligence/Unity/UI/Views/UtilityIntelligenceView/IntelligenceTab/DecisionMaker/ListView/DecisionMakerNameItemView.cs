// <copyright file="DecisionMakerNameItemView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerNameItemView : WinnerStatusNameItemView<DecisionMakerItemViewModel>
    {
        public DecisionMakerNameItemView() : base(true, true, true)
        {
        }

        protected override void OnRegisterViewModelEvents(DecisionMakerItemViewModel viewModel)
        {
            base.OnRegisterViewModelEvents(viewModel);

            OnContextChanged(viewModel.ContextViewModel.Context);
            viewModel.ContextChanged += OnContextChanged;
        }

        protected override void OnUnregisterViewModelEvents(DecisionMakerItemViewModel viewModel)
        {
            base.OnUnregisterViewModelEvents(viewModel);

            viewModel.ContextChanged -= OnContextChanged;
        }

        private void OnContextChanged(DecisionMakerContext context)
        {
            if (!IsRuntime)
                IsWinner = context.IsWinner;
        }
    }
}