// <copyright file="ActionContainerViewDecisionTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ActionContainerViewDecisionTab : UtilityIntelligenceViewMember<DecisionItemViewModel>, IMainView<DecisionSubView>
    {
        private readonly EnumField executionModeField;
        private readonly ActionItemCreatorViewDecisionTab itemCreatorView;
        private readonly ActionListViewDecisionTab listView;

        private readonly Toggle keepRunningUntilFinishedToggle;
        private readonly IntegerField maxRepeatCountField;

        public ActionContainerViewDecisionTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Actions";
            Add(foldout);

            keepRunningUntilFinishedToggle = new("Keep Running Until Finished");
            keepRunningUntilFinishedToggle.tooltip = ToolTips.KeepRunningUntilFinished;
            keepRunningUntilFinishedToggle.RegisterValueChangedCallback(evt => ViewModel.KeepRunningUntilFinished = evt.newValue);
            foldout.Add(keepRunningUntilFinishedToggle);

            maxRepeatCountField = new("Max Repeat Count");
            maxRepeatCountField.isDelayed = true;
            maxRepeatCountField.tooltip = ToolTips.MaxRepeatCount;
            maxRepeatCountField.RegisterValueChangedCallback(evt =>
            {
                int maxRepeatCount = evt.newValue;
                ViewModel.MaxRepeatCount = maxRepeatCount;
            });
            foldout.Add(maxRepeatCountField);

            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
            reorderableToggle.style.borderTopWidth = 1.0f;
            reorderableToggle.style.paddingTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            foldout.Add(reorderableToggle);

            listView = new ActionListViewDecisionTab();
            listView.style.marginTop = 5;
            listView.style.marginBottom = 5;
            foldout.Add(listView);

            executionModeField = new EnumField(ActionExecutionMode.Sequence);
            executionModeField.style.marginTop = 10;
            executionModeField.style.marginBottom = 10;
            executionModeField.RegisterValueChangedCallback(evt =>
            {
                ViewModel.ActionExecutionMode = (ActionExecutionMode)evt.newValue;
            });

            foldout.Add(executionModeField);

            itemCreatorView = new ActionItemCreatorViewDecisionTab();
            itemCreatorView.style.marginTop = 10;
            foldout.Add(itemCreatorView);
        }

        #region IMainView

        public DecisionSubView SubView { get; private set; }

        public void InitSubView(DecisionSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
            listView.ItemAdded += item => { UpdateExecutionModeFieldDisplay(ViewModel); };

            listView.ItemRemoved += item => { UpdateExecutionModeFieldDisplay(ViewModel); };
        }

        #endregion

        #region ActionContainerViewDecisionTab

        protected override void OnUpdateView(DecisionItemViewModel viewModel)
        {
            listView.UpdateView(viewModel?.ActionListViewModel);
            itemCreatorView.UpdateView(viewModel?.ActionListViewModel);
        }

        protected override void OnRefreshView(DecisionItemViewModel viewModel)
        {
            keepRunningUntilFinishedToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(DecisionItemViewModel.KeepRunningUntilFinished),
                BindingMode.ToTarget);

            maxRepeatCountField.SetDataBinding(
                nameof(IntegerField.value),
                nameof(DecisionItemViewModel.MaxRepeatCount),
                BindingMode.ToTarget);

            UpdateExecutionModeField(viewModel);
        }

        protected override void OnResetView()
        {
            keepRunningUntilFinishedToggle.ClearBindings();
            maxRepeatCountField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
            itemCreatorView.RootView = rootView;
        }

        #endregion

        private void UpdateExecutionModeField(DecisionItemViewModel decisionViewModel)
        {
            executionModeField.SetValueWithoutNotify(decisionViewModel.ActionExecutionMode);
            UpdateExecutionModeFieldDisplay(decisionViewModel);
        }

        private void UpdateExecutionModeFieldDisplay(DecisionItemViewModel decisionViewModel)
        {
            if (decisionViewModel.ActionListViewModel.Items.Count >= 2)
                executionModeField.SetDisplay(true);
            else
                executionModeField.SetDisplay(false);
        }

        protected override void OnModelChanged(IModel newModel)
        {
            if (newModel == null) return;

            UpdateExecutionModeField(ViewModel);
        }
    }
}