// <copyright file="DecisionMainViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMainViewIntelligenceTab : NameMainView<DecisionItemViewModelIntelligenceTab, DecisionSubViewIntelligenceTab>
    {
        private TargetFilterContainerViewIntelligenceTab targetFilterContainerView;

        private ActionContainerViewIntelligenceTab actionContainerView;

        private ConsiderationContainerViewIntelligenceTab considerationContainerView;

        private FloatField weightField;

        private Button editButton;

        public DecisionMainViewIntelligenceTab() : base(UIBuilderResourcePaths.DecisionMainViewIntelligenceTab)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            weightField = this.Q<FloatField>("WeightField");
            weightField.SetEnabled(false);

            targetFilterContainerView = this.Q<TargetFilterContainerViewIntelligenceTab>();

            considerationContainerView = this.Q<ConsiderationContainerViewIntelligenceTab>();
            actionContainerView = this.Q<ActionContainerViewIntelligenceTab>();


            editButton = this.Q<Button>("EditButton");
            editButton.clicked += SwitchToEditView;
        }

        protected override void OnInitSubView(DecisionSubViewIntelligenceTab subView)
        {
            targetFilterContainerView.InitSubView(subView);
            actionContainerView.InitSubView(subView);
            considerationContainerView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionItemViewModelIntelligenceTab viewModel)
        {
            targetFilterContainerView.UpdateView(viewModel?.DecisionViewModel);
            actionContainerView.UpdateView(viewModel);
            considerationContainerView.UpdateView(viewModel?.ConsiderationListViewModel);
        }

        protected override void OnRefreshView(DecisionItemViewModelIntelligenceTab viewModel)
        {
            base.OnRefreshView(viewModel);
            weightField.SetDataBinding(nameof(FloatField.value), nameof(DecisionItemViewModelIntelligenceTab.Weight),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            weightField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            targetFilterContainerView.RootView = rootView;
            actionContainerView.RootView = rootView;
            considerationContainerView.RootView = rootView;
        }

        private void SwitchToEditView()
        {
            RootView.SelectDecisionTab(ViewModel.DecisionViewModel.Index);
        }
    }
}