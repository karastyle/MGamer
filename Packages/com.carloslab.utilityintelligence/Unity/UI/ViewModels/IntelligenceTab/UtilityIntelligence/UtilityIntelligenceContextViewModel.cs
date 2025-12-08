// <copyright file="UtilityIntelligenceContextViewModel.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceContextViewModel : UtilityIntelligenceViewModelMember<UtilityIntelligenceModel>, IDataSourceViewHashProvider
    {
        #region Context

        private UtilityIntelligenceContext context;
        public UtilityIntelligenceContext Context => context;

        public event Action<UtilityIntelligenceContext> ContextChanged;

        private void OnContextChanged(UtilityIntelligenceContext context)
        {
            this.context = context;
            ContextChanged?.Invoke(context);
            UpdateViewHashCode();
        }

        #endregion

        #region Model

        protected override void OnRegisterModelEvents(UtilityIntelligenceModel model)
        {
            context = model.Runtime.Context;
            model.Runtime.ContextChanged += OnContextChanged;
        }

        protected override void OnUnregisterModelEvents(UtilityIntelligenceModel model)
        {
            model.Runtime.ContextChanged -= OnContextChanged;
        }

        protected override void OnModelChanged(UtilityIntelligenceModel newModel)
        {
            this.context = UtilityIntelligenceContext.Null;
            UpdateViewHashCode();
        }

        #endregion
    }
}