// <copyright file="DecisionItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionItemCreatorView : NameItemCreatorView<DecisionListViewModel, DecisionItemViewModel>
    {
        protected override void OnItemAdded(DecisionItemViewModel item)
        {
            base.OnItemAdded(item);

            if (ViewModel.Items.Count == 1)
                MakeDecision();
        }

        protected override void OnItemRemoved(DecisionItemViewModel item)
        {
            base.OnItemRemoved(item);

            MakeDecision();
        }

        public void MakeDecision()
        {
            var intelligenceViewModel = ViewModel.RootViewModel;
            intelligenceViewModel.MakeDecision();
        }
    }
}