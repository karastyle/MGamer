// <copyright file="ActionContainerViewIntelligenceTab.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ActionContainerViewIntelligenceTab : UtilityIntelligenceViewMember<DecisionItemViewModelIntelligenceTab>, IMainView<DecisionSubViewIntelligenceTab>
    {
        private readonly EnumField executionModeField;
        private readonly ActionListViewIntelligenceTab listView;

        private readonly Toggle keepRunningUntilFinishedToggle;
        private readonly IntegerField maxRepeatCountField;
        private readonly IntegerField currentRepeatCountField;

        public ActionContainerViewIntelligenceTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Actions";
            Add(foldout);

            keepRunningUntilFinishedToggle = new("Keep Running Until Finished");
            keepRunningUntilFinishedToggle.tooltip = ToolTips.KeepRunningUntilFinished;
            keepRunningUntilFinishedToggle.SetEnabled(false);
            foldout.Add(keepRunningUntilFinishedToggle);

            maxRepeatCountField = new("Max Repeat Count");
            maxRepeatCountField.tooltip = ToolTips.MaxRepeatCount;
            maxRepeatCountField.SetEnabled(false);
            maxRepeatCountField.RegisterValueChangedCallback(evt =>
            {
                UpdateCurrentRepeatCountFieldDisplay();
            });
            foldout.Add(maxRepeatCountField);

            currentRepeatCountField = new("Current Repeat Count");
            currentRepeatCountField.SetEnabled(false);
            currentRepeatCountField.tooltip = ToolTips.CurrentRepeatCount;
            foldout.Add(currentRepeatCountField);

            listView = new();
            listView.style.marginTop = 5;
            listView.style.marginBottom = 5;
            foldout.Add(listView);

            executionModeField = new EnumField(ActionExecutionMode.Sequence);
            executionModeField.style.marginTop = 10;

            executionModeField.SetEnabled(false);
            foldout.Add(executionModeField);
        }

        public DecisionSubViewIntelligenceTab SubView { get; private set; }

        public void InitSubView(DecisionSubViewIntelligenceTab subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
            listView.ItemAdded += item => { UpdateExecutionModeFieldDisplay(); };

            listView.ItemRemoved += item => { UpdateExecutionModeFieldDisplay(); };
        }

        protected override void OnRefreshView(DecisionItemViewModelIntelligenceTab viewModel)
        {
            listView.UpdateView(viewModel?.ActionListViewModel);
            UpdateCurrentRepeatCountField(viewModel);
            UpdateExecutionModeField(viewModel);

            keepRunningUntilFinishedToggle.SetDataBinding(
                nameof(Toggle.value),
                nameof(DecisionItemViewModelIntelligenceTab.KeepRunningUntilFinished),
                BindingMode.ToTarget);

            maxRepeatCountField.SetDataBinding(
                nameof(IntegerField.value),
                nameof(DecisionItemViewModelIntelligenceTab.MaxRepeatCount),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            keepRunningUntilFinishedToggle.ClearBindings();
            maxRepeatCountField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
        }

        private void UpdateCurrentRepeatCountField(DecisionItemViewModelIntelligenceTab decisionViewModel)
        {
            currentRepeatCountField.value = decisionViewModel.CurrentRepeatCount;
            currentRepeatCountField.SetDataBinding(nameof(IntegerField.value), nameof(DecisionItemViewModelIntelligenceTab.CurrentRepeatCount),
                BindingMode.ToTarget);

            UpdateCurrentRepeatCountFieldDisplay();
        }

        private void UpdateCurrentRepeatCountFieldDisplay()
        {
            if (maxRepeatCountField.value > 0)
                currentRepeatCountField.SetDisplay(true);
            else
                currentRepeatCountField.SetDisplay(false);
        }

        private void UpdateExecutionModeField(DecisionItemViewModelIntelligenceTab decisionViewModel)
        {
            executionModeField.value = decisionViewModel.ActionExecutionMode;
            executionModeField.SetDataBinding(nameof(EnumField.value), nameof(DecisionItemViewModelIntelligenceTab.ActionExecutionMode),
                BindingMode.ToTarget);
            UpdateExecutionModeFieldDisplay();
        }

        private void UpdateExecutionModeFieldDisplay()
        {
            if (ViewModel.ActionListViewModel.Items.Count >= 2)
                executionModeField.SetDisplay(true);
            else
                executionModeField.SetDisplay(false);
        }
    }
}