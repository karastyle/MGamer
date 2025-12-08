// <copyright file="NameMainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public abstract class NameMainView<TViewModel, TSubView, TRootView>
        : NameView<TViewModel, TRootView>, IMainView<TSubView>
        where TViewModel : class, INameViewModel
        where TSubView : BaseView, IRootViewMember
        where TRootView : BaseView, IRootView
    {
        protected NameMainView(string visualAssetPath) : base(visualAssetPath)
        {
            // this.SetDisplay(false);
        }

        public TSubView SubView { get; private set; }

        public void InitSubView(TSubView subView)
        {
            SubView = subView;
            OnInitSubView(subView);
        }

        protected virtual void OnInitSubView(TSubView subView)
        {
        }
    }
}