// <copyright file="UtilityIntelligenceViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common.UI;
using Unity.Properties;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceViewModel : RootViewModel<UtilityIntelligenceModel, UtilityIntelligenceAsset>, INameViewModel
    {
        protected override void OnInit(UtilityIntelligenceModel model)
        {
            contextViewModel.RootViewModel = this;
            contextViewModel.Init(model);

            blackboardViewModel.RootViewModel = this;
            blackboardViewModel.Init(model.Blackboard);

            inputListViewModel.RootViewModel = this;
            inputListViewModel.Init(model.Inputs);

            inputNormalizationListViewModel.RootViewModel = this;
            inputNormalizationListViewModel.Init(model.InputNormalizations);

            considerationListViewModel.RootViewModel = this;
            considerationListViewModel.Init(model.Considerations);

            targetFilterListViewModel.RootViewModel = this;
            targetFilterListViewModel.Init(model.TargetFilters);

            decisionListViewModel.RootViewModel = this;
            decisionListViewModel.Init(model.Decisions);

            decisionMakerListViewModel.RootViewModel = this;
            decisionMakerListViewModel.Init(model.DecisionMakers);
        }

        protected override void OnDeinit()
        {
            contextViewModel.Deinit();
            blackboardViewModel.Deinit();
            inputListViewModel.Deinit();
            inputNormalizationListViewModel.Deinit();
            considerationListViewModel.Deinit();
            targetFilterListViewModel.Deinit();
            decisionListViewModel.Deinit();
            decisionMakerListViewModel.Deinit();
        }

        #region ViewModel Properties

        private UtilityIntelligenceContextViewModel contextViewModel = new();
        public UtilityIntelligenceContextViewModel ContextViewModel => contextViewModel;

        private BlackboardViewModel blackboardViewModel = new();
        public BlackboardViewModel BlackboardViewModel => blackboardViewModel;

        private DecisionMakerListViewModel decisionMakerListViewModel = new();
        public DecisionMakerListViewModel DecisionMakerListViewModel => decisionMakerListViewModel;


        private DecisionListViewModel decisionListViewModel = new();
        public DecisionListViewModel DecisionListViewModel => decisionListViewModel;


        private TargetFilterListViewModel targetFilterListViewModel = new();
        public TargetFilterListViewModel TargetFilterListViewModel => targetFilterListViewModel;


        private ConsiderationListViewModel considerationListViewModel = new();
        public ConsiderationListViewModel ConsiderationListViewModel => considerationListViewModel;


        private InputListViewModel inputListViewModel = new();
        public InputListViewModel InputListViewModel => inputListViewModel;


        private InputNormalizationListViewModel inputNormalizationListViewModel = new();
        public InputNormalizationListViewModel InputNormalizationListViewModel => inputNormalizationListViewModel;


        #endregion

        #region Binding Properties

        private string name = UtilityIntelligenceUIConsts.DefaultItemName;
        [CreateProperty]
        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;

                name = value;
                Notify();
            }
        }

        [CreateProperty]
        public bool EnableCompensationFactor
        {

            get => Model?.EnableCompensationFactor ?? false;
            set
            {
                if (Model == null || Model.EnableCompensationFactor == value) return;

                Record($"UtilityIntelligenceViewModel EnableCompensationFactor Changed: {value}",
                    () => { Model.EnableCompensationFactor = value; });

                MakeDecision();
            }
        }

        [CreateProperty]
        public float MomentumBonus
        {

            get => Model?.MomentumBonus ?? 0;
            set
            {
                if (Model == null) return;

                Record($"UtilityIntelligenceViewModel MomentumBonus Changed: {value}",
                    () => { Model.MomentumBonus = value; });

                MakeDecision();
            }
        }

        #endregion

        protected override void OnModelChanged(UtilityIntelligenceModel newModel)
        {
            ContextViewModel.Model = newModel;

            BlackboardViewModel.Model = newModel?.Blackboard;
            InputListViewModel.Model = newModel?.Inputs;
            InputNormalizationListViewModel.Model = newModel?.InputNormalizations;
            ConsiderationListViewModel.Model = newModel?.Considerations;
            TargetFilterListViewModel.Model = newModel?.TargetFilters;
            DecisionListViewModel.Model = newModel?.Decisions;
            DecisionMakerListViewModel.Model = newModel?.DecisionMakers;

            if (newModel == null) return;

            Notify(nameof(Name));
            Notify(nameof(EnableCompensationFactor));
            Notify(nameof(MomentumBonus));

            MakeDecision();
        }

        internal void MakeDecision()
        {
            // Debug.Log("MakeDecision");
            if (IsRuntime || Model == null) return;

            Model.Runtime.MakeDecision(null);
        }
    }
}