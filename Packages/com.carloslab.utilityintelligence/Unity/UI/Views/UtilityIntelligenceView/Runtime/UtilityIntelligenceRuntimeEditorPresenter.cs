// <copyright file="UtilityIntelligenceRuntimeEditorPresenter.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.UI
{
    [RequireComponent(typeof(UtilityAgentController))]
    [AddComponentMenu(FrameworkConsts.AddRuntimeEditorPresenterMenuPath)]
    public class UtilityIntelligenceRuntimeEditorPresenter : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField]
        private UtilityIntelligenceRuntimeEditor editor;

        [SerializeField]
        private KeyCode showKey;

        private UtilityAgentController agent;

        public UtilityIntelligenceAsset Asset => agent.Asset;

        public bool IsRuntimeEditorOpening => isInitialized && Asset.IsRuntimeEditorOpening;

        private bool isInitialized;

        #region ViewModel

        private UtilityIntelligenceViewModel viewModel;

        #endregion

        private void Awake()
        {
            agent = GetComponent<UtilityAgentController>();
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (agent.EditorAsset == null)
            {
                StaticConsole.LogError($"{name} contains null IntelligenceAsset");
                isInitialized = false;
                return;
            }

            if (editor == null)
            {
                StaticConsole.LogError($"{name} contains null ViewController");
                isInitialized = false;
                return;
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(showKey))
            {
                OpenEditor();
            }
        }

        private void OnDestroy()
        {
            CloseEditor();
        }

        private void OpenEditor()
        {
            if (IsRuntimeEditorOpening) return;

            viewModel = editor.CreateViewModel(Asset);
            viewModel.Name = name;
            editor.ShowView(viewModel);
        }

        private void CloseEditor()
        {
            if (!IsRuntimeEditorOpening) return;

            editor.HideView();
        }
#endif
    }
}