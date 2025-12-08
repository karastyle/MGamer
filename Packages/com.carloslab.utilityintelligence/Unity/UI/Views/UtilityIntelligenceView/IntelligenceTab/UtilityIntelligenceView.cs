// <copyright file="UtilityIntelligenceView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class UtilityIntelligenceView : RootView<UtilityIntelligenceViewModel>
    {
        private TabView intelligenceTabView;
        private Tab intelligenceTab;
        public bool IsIntelligenceTabActive => intelligenceTabView.activeTab == intelligenceTab;
        private IntelligenceTabMainView intelligenceTabMainView;

        private Tab decisionTab;
        private DecisionTabMainView decisionTabMainView;
        public DecisionListView DecisionListView => decisionTabMainView.ContainerView.ListView;

        private Tab targetFilterTab;
        private TargetFilterTabMainView targetFilterTabMainView;
        public TargetFilterListView TargetFilterListView => targetFilterTabMainView.ListView;

        private Tab considerationTab;
        private ConsiderationTabMainView considerationTabMainView;
        public ConsiderationListView ConsiderationListView => considerationTabMainView.ListView;

        private Tab inputTab;
        private InputTabMainView inputTabMainView;

        private Tab inputNormalizationTab;
        private InputNormalizationTabMainView inputNormalizationTabMainView;

        private Tab blackboardTab;
        private BlackboardTabMainView blackboardTabMainView;
        public BlackboardTabMainView BlackboardTabMainView => blackboardTabMainView;

        private bool styleSheetsLoaded;

        public UtilityIntelligenceView() : this(false)
        {

        }

        public UtilityIntelligenceView(bool isRuntimeUI) : base(isRuntimeUI, UIBuilderResourcePaths.IntelligenceView)
        {
            style.flexGrow = 1;
            //
            // if(hasLoadedVisualAsset)
            //     InitView();

            LoadStyleSheets(isRuntimeUI);
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();


            intelligenceTabView = this.Q<TabView>();
            intelligenceTab = intelligenceTabView.Q<Tab>("IntelligenceTab");
            decisionTab = intelligenceTabView.Q<Tab>("DecisionTab");
            targetFilterTab = intelligenceTabView.Q<Tab>("TargetFilterTab");
            considerationTab = intelligenceTabView.Q<Tab>("ConsiderationTab");
            inputTab = intelligenceTabView.Q<Tab>("InputTab");
            inputNormalizationTab = intelligenceTabView.Q<Tab>("InputNormalizationTab");
            blackboardTab = intelligenceTabView.Q<Tab>("BlackboardTab");

        }

        #region TabView

        internal void InitView()
        {
            IntelligenceTabSplitView intelligenceTabSplitView = new();
            intelligenceTabSplitView.RootView = this;
            intelligenceTabMainView = intelligenceTabSplitView.MainView;
            intelligenceTab.Add(intelligenceTabSplitView);

            DecisionTabSplitView decisionTabSplitView = new();
            decisionTabSplitView.RootView = this;
            decisionTabMainView = decisionTabSplitView.MainView;
            decisionTab.Add(decisionTabSplitView);

            TargetFilterTabSplitView targetFilterSplitView = new();
            targetFilterSplitView.RootView = this;
            targetFilterTabMainView = targetFilterSplitView.MainView;
            targetFilterTab.Add(targetFilterSplitView);

            ConsiderationTabSplitView considerationsSplitView = new();
            considerationsSplitView.RootView = this;
            considerationTabMainView = considerationsSplitView.MainView;
            considerationTab.Add(considerationsSplitView);

            InputTabSplitView inputTabSplitView = new();
            inputTabSplitView.RootView = this;
            inputTabMainView = inputTabSplitView.MainView;
            inputTab.Add(inputTabSplitView);

            InputNormalizationTabSplitView inputNormalizationTabSplitView = new();
            inputNormalizationTabSplitView.RootView = this;

            inputNormalizationTabMainView = inputNormalizationTabSplitView.MainView;
            inputNormalizationTab.Add(inputNormalizationTabSplitView);

            BlackboardTabSplitView blackboardTabSplitView = new();
            blackboardTabSplitView.RootView = this;
            blackboardTabMainView = blackboardTabSplitView.MainView;
            blackboardTab.Add(blackboardTabSplitView);
        }

        internal void ClearView()
        {
            intelligenceTab.Clear();
            decisionTab.Clear();
            targetFilterTab.Clear();
            considerationTab.Clear();
            inputTab.Clear();
            inputNormalizationTab.Clear();
            blackboardTab.Clear();
        }

        internal void ClearViewModel()
        {
            ViewModel = null;
        }

        protected override void OnUpdateView(UtilityIntelligenceViewModel viewModel)
        {
            intelligenceTabMainView.UpdateView(viewModel);
            decisionTabMainView.UpdateView(viewModel?.DecisionListViewModel);
            targetFilterTabMainView.UpdateView(viewModel?.TargetFilterListViewModel);
            considerationTabMainView.UpdateView(viewModel?.ConsiderationListViewModel);
            inputTabMainView.UpdateView(viewModel?.InputListViewModel);
            inputNormalizationTabMainView.UpdateView(viewModel?.InputNormalizationListViewModel);
            blackboardTabMainView.UpdateView(viewModel?.BlackboardViewModel);
        }

        private void LoadStyleSheets(bool isRuntimeUI)
        {
            if (styleSheetsLoaded) return;

#if UNITY_EDITOR
            if (isRuntimeUI || !EditorGUIUtility.isProSkin)
            {
                LoadStyleSheet(UIBuilderResourcePaths.IntelligenceView_Light);
            }
            else
            {
                LoadStyleSheet(UIBuilderResourcePaths.IntelligenceView_Dark);
            }
#else
            LoadStyleSheet(UIBuilderResourcePaths.IntelligenceView_Light);
#endif

            styleSheetsLoaded = true;
        }
        // public void UpdateModel()
        // {
        //     ViewModel.Model = ViewModel.Asset.Model;
        //     // ResetTabView();
        //     // UpdateView(viewModel);
        // }


        #endregion

        public void SelectBlackboardTab()
        {
            intelligenceTabView.activeTab = blackboardTab;
        }

        public void SelectDecisionTab(int itemIndex)
        {
            SelectDecisionTab();
            DecisionListView.SelectedIndex = itemIndex;
        }

        public void SelectDecisionTab()
        {
            intelligenceTabView.activeTab = decisionTab;
        }

        public void SelectTargetFilterTab(int itemIndex)
        {
            SelectTargetFilterTab();
            TargetFilterListView.SelectedIndex = itemIndex;
        }

        public void SelectTargetFilterTab()
        {
            intelligenceTabView.activeTab = targetFilterTab;
        }

        public void SelectConsiderationTab(int itemIndex)
        {
            SelectConsiderationTab();
            ConsiderationListView.SelectedIndex = itemIndex;
        }

        public void SelectConsiderationTab()
        {
            intelligenceTabView.activeTab = considerationTab;
        }

        public void SelectInputNormalizationTab()
        {
            intelligenceTabView.activeTab = inputNormalizationTab;
        }

        public void SelectInputTab()
        {
            intelligenceTabView.activeTab = inputTab;
        }
    }
}