// <copyright file="IntelligenceTabMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class IntelligenceTabMainView : NameMainView<UtilityIntelligenceViewModel, IntelligenceTabSubView>
    {
        private Toggle compensationFactorToggle;
        private FloatField momentumBonusField;

        private DecisionMakerContainerView decisionMakerContainerView;

        public IntelligenceTabMainView() : base(UIBuilderResourcePaths.IntelligenceTabMainView)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            compensationFactorToggle = this.Query<Toggle>("CompensationFactorToggle");
            compensationFactorToggle.RegisterValueChangedCallback(evt => ViewModel.EnableCompensationFactor = evt.newValue);

            momentumBonusField = this.Query<FloatField>("MomentumBonusField");
            momentumBonusField.isDelayed = true;
            momentumBonusField.RegisterValueChangedCallback(evt =>
            {
                var newValue = evt.newValue;
                var oldValue = evt.previousValue;
                if (newValue < 0.0f)
                {
                    momentumBonusField.SetValueWithoutNotify(oldValue);
                }
                else
                {
                    ViewModel.MomentumBonus = newValue;
                }
            });

            decisionMakerContainerView = this.Q<DecisionMakerContainerView>();
        }

        protected override void OnInitSubView(IntelligenceTabSubView subView)
        {
            decisionMakerContainerView?.InitSubView(subView);
        }

        protected override void OnUpdateView(UtilityIntelligenceViewModel viewModel)
        {
            decisionMakerContainerView.UpdateView(viewModel?.DecisionMakerListViewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            decisionMakerContainerView.RootView = rootView;
        }

        protected override void OnRefreshView(UtilityIntelligenceViewModel viewModel)
        {
            base.OnRefreshView(viewModel);

            compensationFactorToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(UtilityIntelligenceViewModel.EnableCompensationFactor),
                BindingMode.ToTarget);

            momentumBonusField.SetDataBinding(
                nameof(Toggle.value),
                nameof(UtilityIntelligenceViewModel.MomentumBonus),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            compensationFactorToggle.ClearBindings();
            momentumBonusField.ClearBindings();
        }
    }
}