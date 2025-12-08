// <copyright file="MainTargetScoreListView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class MainBasicListView<TListViewModel, TItemViewModel, TSubView> :
        MainBasicListView<TListViewModel, TItemViewModel, TSubView, UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>,
        IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TSubView : BaseView
    {
        protected MainBasicListView() : base(null)
        {
        }
    }
    public abstract class MainTargetScoreListView<TListViewModel, TItemViewModel, TSubView> :
        MainBasicListView<TListViewModel, TItemViewModel, TSubView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TSubView : BaseView
    {

        #region MultiColumn

        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterTargetColumn(listView);
            RegisterScoreColumn(listView);
            RegisterControlsColumn(listView);
        }

        protected override VisualElement OnMakeCell(string columnName)
        {
            VisualElement cell = null;
            switch (columnName)
            {
                case ScoreColumnName:
                    cell = MakeScoreCell();
                    break;
                case TargetColumnName:
                    cell = MakeTargetCell();
                    break;
                default:
                    cell = base.OnMakeCell(columnName);
                    break;
            }

            return cell;
        }

        protected override void OnBindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case ScoreColumnName:
                    BindCellScore(element, index);
                    break;
                case TargetColumnName:
                    BindCellTarget(element, index);
                    break;
                default:
                    base.OnBindCell(columnName, element, index);
                    break;
            }
        }

        protected override void OnUnbindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case ScoreColumnName:
                    UnbindCellScore(element, index);
                    break;
                case TargetColumnName:
                    UnbindCellTarget(element, index);
                    break;
                default:
                    base.OnUnbindCell(columnName, element, index);
                    break;
            }
        }

        #endregion

        #region ScoreColumn

        private const string ScoreColumnName = "Score";

        protected virtual string ScoreColumnTitle => "Score";
        protected virtual float ScoreColumnWidth => 50;

        protected void RegisterScoreColumn(MultiColumnListView listView)
        {
            Column scoreColumn = RegisterColumn(listView, ScoreColumnName, ScoreColumnTitle);
            scoreColumn.width = ScoreColumnWidth;
        }

        protected virtual VisualElement MakeScoreCell()
        {
            return new ScoreItemView<TItemViewModel>();
        }

        protected virtual void BindCellScore(VisualElement element, int index)
        {
            ScoreItemView<TItemViewModel> itemView = element as ScoreItemView<TItemViewModel>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        protected virtual void UnbindCellScore(VisualElement element, int index)
        {
            ScoreItemView<TItemViewModel> itemView = element as ScoreItemView<TItemViewModel>;
            itemView?.UpdateView(null);
        }

        #endregion

        #region Target Column

        private const string TargetColumnName = "Target";

        protected virtual string TargetColumnTitle => "Target";

        protected void RegisterTargetColumn(MultiColumnListView listView)
        {
            Column targetColumn = RegisterColumn(listView, TargetColumnName, TargetColumnTitle);
            targetColumn.stretchable = true;
        }

        protected virtual VisualElement MakeTargetCell()
        {
            return new TargetNameItemView<TItemViewModel>();
        }

        protected virtual void BindCellTarget(VisualElement element, int index)
        {
            TargetNameItemView<TItemViewModel> itemView = element as TargetNameItemView<TItemViewModel>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        protected virtual void UnbindCellTarget(VisualElement element, int index)
        {
            TargetNameItemView<TItemViewModel> itemView = element as TargetNameItemView<TItemViewModel>;
            itemView?.UpdateView(null);
        }

        #endregion

    }
}