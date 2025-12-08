// <copyright file="ObjectEditorView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using CarlosLab.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ObjectEditorView<TViewModel> : UtilityIntelligenceViewMember<TViewModel>
        where TViewModel : class, IRootViewModelMember<UtilityIntelligenceViewModel>

    {
        private HashSet<string> showIfFieldNames = new();
        private Dictionary<string, Foldout> foldoutGroups = new();
        private Dictionary<string, GroupBox> boxGroups = new();
        public ObjectEditorView() : base(null)
        {
        }

        public event Action<GenericField> FieldValueChanged;

        protected override void OnUpdateView(TViewModel viewModel)
        {
            if (viewModel.ModelObject is GenericModel model) UpdateView(model);
        }

        protected override void OnModelChanged(IModel newModel)
        {
            if (newModel is GenericModel genericModel)
                UpdateView(genericModel);
        }

        private void UpdateView(GenericModel model)
        {
            Reset();

            var publicFieldInfos = model.PublicFieldInfos;
            object runtimeObject = model.RuntimeObject;

            foreach (var fieldInfo in publicFieldInfos.Values)
            {
                bool visible = GetVisible(fieldInfo, model);

                if (!visible)
                    continue;

                GenericField field = new(fieldInfo.FieldType, false, fieldInfo.Name);
                if (!field.IsValid)
                    continue;

                field.RootView = RootView;
                object fieldValue = fieldInfo.GetValue(runtimeObject);
                field.Value = fieldValue;

                field.dataSource = runtimeObject;
                field.SetValueDataBinding(fieldInfo.Name, BindingMode.ToTarget);

                field.ValueChanged += newValue =>
                {
                    model.SetValueRuntime(fieldInfo.Name, field.Value);
                    FieldValueChanged?.Invoke(field);

                    if (showIfFieldNames.Contains(fieldInfo.Name))
                        UpdateView(model);
                };

                field.InputApplied += () =>
                {
                    object fieldValue = field.Value;
                    object newValue = field.Value;
                    if (field.Value is IVariableReference variableReference)
                    {
                        fieldValue = variableReference.Clone();
                        newValue = variableReference.ValueObject;
                    }

                    ViewModel.Record($"ObjectEditorView Field: {fieldInfo.Name} Changed, NewValue: {newValue}",
                        () => { model.SetValueWithoutRuntime(fieldInfo.Name, fieldValue); });
                };

                if (TryGetFoldout(fieldInfo, out Foldout foldout))
                {
                    foldout.Add(field);
                }
                else if (TryGetBoxGroup(fieldInfo, out GroupBox boxGroup))
                {
                    boxGroup.Add(field);
                }
                else
                {
                    Add(field);
                }
            }
        }

        private bool TryGetFoldout(FieldInfo fieldInfo, out Foldout foldout)
        {
            foldout = null;

            var foldoutGroupAttribute = fieldInfo.GetCustomAttribute<FoldoutGroupAttribute>();
            if (foldoutGroupAttribute != null)
            {
                string foldoutGroupName = foldoutGroupAttribute.GroupName;
                if (!string.IsNullOrWhiteSpace(foldoutGroupName) && !foldoutGroups.TryGetValue(foldoutGroupName, out foldout))
                {
                    foldout = new Foldout();
                    foldout.text = foldoutGroupName;
                    foldoutGroups.Add(foldoutGroupName, foldout);
                    Add(foldout);
                }

                return true;
            }

            return false;
        }

        private bool TryGetBoxGroup(FieldInfo fieldInfo, out GroupBox boxGroup)
        {
            boxGroup = null;

            var boxGroupAttribute = fieldInfo.GetCustomAttribute<BoxGroupAttribute>();
            if (boxGroupAttribute != null)
            {
                string boxGroupName = boxGroupAttribute.GroupName;
                if (!string.IsNullOrWhiteSpace(boxGroupName) && !boxGroups.TryGetValue(boxGroupName, out boxGroup))
                {
                    boxGroup = new GroupBox();
                    boxGroup.text = boxGroupName;
                    boxGroups.Add(boxGroupName, boxGroup);
                    Add(boxGroup);
                }

                return true;
            }

            return false;
        }

        private bool GetVisible(FieldInfo fieldInfo, GenericModel model)
        {
            var showIfAttribute = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            if (showIfAttribute != null)
            {
                return GetVisibleShowIf(showIfAttribute, model);
            }

            var hideIfAttribute = fieldInfo.GetCustomAttribute<HideIfAttribute>();
            if (hideIfAttribute != null)
            {
                return GetVisibleHideIf(hideIfAttribute, model);
            }
            return true;
        }

        private bool GetVisibleShowIf(ShowIfAttribute showIfAttribute, GenericModel model)
        {
            bool visible = false;
            var fieldInfos = model.FieldInfos;
            object runtimeObject = model.RuntimeObject;

            string showIfFieldName = showIfAttribute.FieldName;
            object showIfFieldValue = showIfAttribute.FieldValue;
            if (fieldInfos.TryGetValue(showIfFieldName, out FieldInfo targetFieldInfo))
            {
                showIfFieldNames.Add(showIfFieldName);
                var targetFieldValue = targetFieldInfo.GetValue(runtimeObject);
                if (showIfFieldValue != null)
                {
                    if (targetFieldValue.Equals(showIfFieldValue))
                        visible = true;
                    else
                        visible = false;
                }
                else
                {
                    if (targetFieldValue is bool boolValue)
                    {
                        if (boolValue)
                            visible = true;
                    }
                }
            }

            return visible;
        }

        private bool GetVisibleHideIf(HideIfAttribute hideIfAttriute, GenericModel model)
        {
            bool visible = false;
            var fieldInfos = model.FieldInfos;
            object runtimeObject = model.RuntimeObject;

            string hideIfFieldName = hideIfAttriute.FieldName;
            object hideIfFieldValue = hideIfAttriute.FieldValue;
            if (fieldInfos.TryGetValue(hideIfFieldName, out FieldInfo targetFieldInfo))
            {
                showIfFieldNames.Add(hideIfFieldName);
                var targetFieldValue = targetFieldInfo.GetValue(runtimeObject);
                if (hideIfFieldValue != null)
                {
                    if (targetFieldValue.Equals(hideIfFieldValue))
                        visible = false;
                    else
                        visible = true;
                }
                else
                {
                    if (targetFieldValue is bool boolValue)
                    {
                        if (!boolValue)
                            visible = true;
                    }
                }
            }

            return visible;
        }


        public void Reset()
        {
            Clear();

            showIfFieldNames.Clear();
            foldoutGroups.Clear();
            boxGroups.Clear();
        }
    }
}