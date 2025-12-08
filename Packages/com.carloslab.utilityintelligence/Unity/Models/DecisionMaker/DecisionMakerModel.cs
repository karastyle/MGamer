// <copyright file="DecisionMakerModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Extensions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class DecisionMakerModel : ContainerItemModel<DecisionMakerContainerModel, DecisionMaker>
    {
        #region Decisions

        private DecisionContainerModel decisionContainer;

        public DecisionContainerModel DecisionContainer
        {
            get => decisionContainer;
            internal set => decisionContainer = value;
        }

        [DataMember(Name = nameof(Decisions))]
        private List<string> decisionNames = new();


        private List<DecisionModel> decisions;

        public IReadOnlyList<DecisionModel> Decisions
        {
            get
            {
                if (decisions == null)
                {
                    decisions = new();

                    if (decisionContainer != null)
                    {
                        foreach (string decisionName in decisionNames)
                        {
                            if (decisionContainer.TryGetItem(decisionName, out DecisionModel decision))
                                decisions.Add(decision);
                            else
                                StaticConsole.LogWarning($"Asset: {Asset?.Name} DecisionMaker: {Name} Cannot find the Decision: {decisionName}. Please remove it from your JSON using File Menu Toolbar.");
                        }
                    }
                }

                return decisions;
            }
        }

        public bool HasDecision(string name)
        {
            return Runtime.HasDecision(name);
        }

        public bool TryAddDecision(int index, string name, DecisionModel decision)
        {
            if (Runtime.TryAddDecision(index, name, decision.Runtime))
            {
                decisionNames.Insert(index, name);
                decisions?.Insert(index, decision);
                return true;
            }

            return false;
        }

        public DecisionModel GetDecisionById(string id)
        {
            return decisions.Find(d => d.Id == id);
        }

        public bool TryRemoveDecision(string name, DecisionModel decision)
        {
            if (Runtime.TryRemoveDecision(name))
            {
                decisionNames.Remove(name);
                decisions.Remove(decision);
                return true;
            }

            return false;
        }

        public bool TryChangeDecisionName(string oldName, string newName)
        {
            int index = decisionNames.IndexOf(oldName);
            if (index >= 0)
            {
                decisionNames[index] = newName;
                return true;
            }

            return false;
        }

        public void MoveDecision(int sourceIndex, int destIndex)
        {
            decisionNames.Move(sourceIndex, destIndex);
            decisions.Move(sourceIndex, destIndex);
            Runtime.MoveDecision(sourceIndex, destIndex);
        }

        #endregion

        #region Runtime

        private DecisionMaker runtime;

        public override DecisionMaker Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = new DecisionMaker();

                    for (int index = 0; index < Decisions.Count; index++)
                    {
                        DecisionModel decision = Decisions[index];
                        runtime.TryAddDecision(decision.Name, decision.Runtime);
                    }
                }

                return runtime;
            }
        }

        #endregion
    }
}