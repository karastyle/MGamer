// <copyright file="ObjectNameEditorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ObjectNameEditorView<TViewModel> : UtilityIntelligenceViewMember<TViewModel>
        where TViewModel : class, ITypeNameViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        private readonly ObjectEditorView<TViewModel> editorView;
        private readonly Label nameLabel;

        public ObjectNameEditorView() : base(null)
        {
            nameLabel = UIElementsFactory.CreateTitleLabel();
            Add(nameLabel);

            ScrollView scollView = new();
            Add(scollView);

            editorView = new ObjectEditorView<TViewModel>();
            scollView.Add(editorView);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            editorView.RootView = RootView;
        }

        protected sealed override void OnUpdateView(TViewModel viewModel)
        {
            editorView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(TViewModel viewModel)
        {
            nameLabel.text = viewModel.TypeName;
        }
    }
}