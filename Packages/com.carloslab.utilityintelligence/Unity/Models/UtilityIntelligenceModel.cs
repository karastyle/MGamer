// <copyright file="UtilityIntelligenceModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class UtilityIntelligenceModel : Model<UtilityIntelligence>, IRootModel<UtilityIntelligence>
    {
        #region Data Asset

        public bool IsRuntimeEditorOpening
        {
            get => Runtime.IsRuntimeEditorOpening;
            set => Runtime.IsRuntimeEditorOpening = value;
        }

        public int EditorOpeningCount
        {
            get => Runtime.EditorOpeningCount;
            set => Runtime.EditorOpeningCount = value;
        }

        public bool IsEditorOpening => Runtime.IsEditorOpening;

        public bool IsRuntime
        {
            get => Runtime.IsRuntime;
            set => Runtime.IsRuntime = value;
        }

        #endregion

        #region Version

        [DataMember(Name = nameof(DataVersion))]
        private int dataVersion;

        public int DataVersion
        {
            get => dataVersion;
            set => dataVersion = value;
        }

        #endregion

        #region Blackboard

        [DataMember(Name = nameof(Blackboard))]
        private BlackboardModel blackboard = new();

        public BlackboardModel Blackboard => blackboard;

        #endregion

        #region DecisionMakers

        [DataMember(Name = nameof(DecisionMakers))]
        private DecisionMakerContainerModel decisionMakers = new();

        public DecisionMakerContainerModel DecisionMakers => decisionMakers;

        #endregion

        #region Decisions

        [DataMember(Name = nameof(Decisions))]
        private DecisionContainerModel decisions = new();

        public DecisionContainerModel Decisions => decisions;

        #endregion

        #region TargetFilters

        [DataMember(Name = nameof(TargetFilters))]
        private TargetFilterContainerModel targetFilters = new();

        public TargetFilterContainerModel TargetFilters => targetFilters;

        #endregion

        #region Considerations

        [DataMember(Name = nameof(Considerations))]
        private ConsiderationContainerModel considerations = new();

        public ConsiderationContainerModel Considerations => considerations;

        [DataMember(Name = nameof(EnableCompensationFactor))]
        private bool enableCompensationFactor = true;
        public bool EnableCompensationFactor
        {
            get => enableCompensationFactor;
            set
            {
                enableCompensationFactor = value;
                Runtime.EnableCompensationFactor = enableCompensationFactor;
            }
        }

        [DataMember(Name = nameof(MomentumBonus))]
        private float momentumBonus = 1.1f;
        public float MomentumBonus
        {
            get => momentumBonus;
            set
            {
                momentumBonus = value;
                Runtime.MomentumBonus = momentumBonus;
            }
        }

        #endregion

        #region Inputs

        [DataMember(Name = nameof(Inputs))]
        private InputContainerModel inputs = new();

        public InputContainerModel Inputs => inputs;


        #endregion

        #region Input Normalizations

        [DataMember(Name = nameof(InputNormalizations))]
        private InputNormalizationContainerModel inputNormalizations = new();

        public InputNormalizationContainerModel InputNormalizations => inputNormalizations;


        #endregion

        #region Runtime
        private UtilityIntelligence runtime;

        public override UtilityIntelligence Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = new UtilityIntelligence(decisionMakers.Runtime, decisions.Runtime, targetFilters.Runtime
                        , considerations.Runtime
                        , inputs.Runtime
                        , inputNormalizations.Runtime
                        , blackboard.Runtime)
                    {
                        EnableCompensationFactor = enableCompensationFactor,
                        MomentumBonus = momentumBonus
                    };
                }

                return runtime;
            }
        }

        #endregion

        #region Init

        [OnDeserialized]
        public void OnIntelligenceDeserialized(StreamingContext context)
        {
            Init();
        }

        private void Init()
        {
            UpdateVariableReferences(targetFilters.Items);
            UpdateVariableReferences(inputs.Items);

            UpdateContainerReferences();
        }

        private void UpdateContainerReferences()
        {
            var decisionMakers = this.decisionMakers.Items;
            for (int index = 0; index < decisionMakers.Count; index++)
            {
                DecisionMakerModel model = decisionMakers[index];
                model.DecisionContainer = this.decisions;
            }
            var decisions = this.decisions.Items;
            for (int index = 0; index < decisions.Count; index++)
            {
                DecisionModel model = decisions[index];

                UpdateVariableReferences(model.Actions);

                model.TargetFilterContainer = this.targetFilters;
                model.ConsiderationContainer = this.considerations;

            }

            var considerations = this.considerations.Items;
            for (int index = 0; index < considerations.Count; index++)
            {
                ConsiderationModel model = considerations[index];
                model.InputNormalizationContainer = this.inputNormalizations;
            }

            var inputNormalizations = this.inputNormalizations.Items;
            for (int index = 0; index < inputNormalizations.Count; index++)
            {
                InputNormalizationModel model = inputNormalizations[index];
                UpdateVariableReferences(model);
                model.InputContainer = this.inputs;
            }
        }

        private void UpdateVariableReferences(IReadOnlyList<IGenericModel> models)
        {
            Blackboard blackboard = this.blackboard.Runtime;
            if (blackboard == null) return;

#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("UtilityIntelligenceAsset - UpdateVariableReferences");
#endif

            for (int modelIndex = 0; modelIndex < models.Count; modelIndex++)
            {
                IGenericModel model = models[modelIndex];
                UpdateVariableReferences(model);
            }
        }

        private void UpdateVariableReferences(IGenericModel model)
        {
            Blackboard blackboard = this.blackboard.Runtime;
            if (blackboard == null) return;

#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("UtilityIntelligenceAsset - UpdateVariableReferences");
#endif

            model.SetVariableReference(blackboard);
        }

        #endregion
    }
}