// <copyright file="BaseTypeItemCreatorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CarlosLab.Common.UI
{
    public abstract class
        BaseTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView> : BaseItemCreatorView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        private PopupField<Type> typeField;

        private VisualElement typeFieldContainer;

        protected virtual string TypeFieldLabel { get; } = "Type";

        protected virtual Type BaseType { get; }
        protected virtual HashSet<Type> ExcludedTypes { get; } = null;

        public BaseTypeItemCreatorView(string visualAssetPath = UIBuilderResourcePaths.TypeItemCreatorView) : base(
            visualAssetPath)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            typeFieldContainer = this.Q<VisualElement>("TypeFieldContainer");

            CreateTypeField(TypeFieldLabel);
            HandleTypeFieldValueChanged();
        }

        protected override void OnRefreshView(TListViewModel viewModel)
        {
            UpdateTypeFieldChoices(BaseType, ExcludedTypes);
        }

        protected PopupField<Type> TypeField => typeField;

        private void CreateTypeField(string label)
        {
            typeField = new()
            {
                label = label
            };

            typeField.formatListItemCallback = FormatListItem;
            typeField.formatSelectedValueCallback = FormatSelectedItem;

            typeFieldContainer.Add(typeField);
        }

        protected virtual string FormatListItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            return itemType.Name;
        }

        protected virtual string FormatSelectedItem(Type itemType)
        {
            if (itemType == null)
                return "None";

            return itemType.Name;
        }

        private void UpdateTypeFieldChoices(Type baseType, HashSet<Type> excludedTypes)
        {
            if (baseType == null) return;

            List<Type> types = null;

#if UNITY_EDITOR
            types = TypeCache.GetTypesDerivedFrom(baseType).ToList();
#else
            types = new();
#endif
            foreach (Type type in types)
            {
                if (type.IsGenericType || type.IsAbstract || (excludedTypes != null && excludedTypes.Contains(type))) continue;

                typeField.choices.Add(type);
            }

            typeField.choices.Sort(CompareChoices);

            // if (typeField.choices.Count > 0 && typeField.value == null)
            //     typeField.value = typeField.choices[0];
        }

        protected virtual int CompareChoices(Type choice1, Type choice2)
        {
            return string.CompareOrdinal(choice1.Name, choice1.Name);
        }

        private void HandleTypeFieldValueChanged()
        {
            typeField.RegisterValueChangedCallback(evt =>
            {
                OnTypeFieldValueChanged(evt.newValue);
            });
        }

        protected virtual void OnTypeFieldValueChanged(Type newType)
        {

        }

        protected override void CreateNewItem()
        {
            Type type = typeField.value;
            if (type != null) ViewModel.CreateItem(type);
        }
    }
}