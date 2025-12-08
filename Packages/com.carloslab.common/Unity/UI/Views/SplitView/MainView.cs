// <copyright file="MainView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

namespace CarlosLab.Common.UI
{
    public class MainView<TViewModel, TSubView, TRootView> : RootViewMember<TViewModel, TRootView>, IMainView<TSubView>
        where TViewModel : class, IViewModel
        where TSubView : BaseView, IRootViewMember
        where TRootView : BaseView, IRootView
    {
        public MainView(string visualAssetPath) : base(visualAssetPath)
        {
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