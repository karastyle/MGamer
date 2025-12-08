// <copyright file="UtilityIntelligenceViewMember.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceViewMember : RootViewMember<UtilityIntelligenceView>
    {
        public UtilityIntelligenceViewMember() : base(null)
        {
        }
    }
    public abstract class UtilityIntelligenceViewMember<TViewModel> : RootViewMember<TViewModel, UtilityIntelligenceView>
        where TViewModel : class, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        public UtilityIntelligenceViewMember(string visualAssetPath) : base(visualAssetPath)
        {
        }

        public void SelectionDecisionTab()
        {
            RootView?.SelectDecisionTab();
        }
    }
}