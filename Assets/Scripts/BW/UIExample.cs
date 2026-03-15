// UIExample.cs

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTools
{
    public class UIExample : MonoBehaviour
    {
        public Text progressText;
        public Image progress;
        public string targetSceneName = "GameScene";

        private PatchUpdaterMono _patchUpdater;
        private bool _completed = false;

        void Start()
        {
            _patchUpdater = GlobalInitializer.Instance.GetModule<PatchUpdaterMono>();
        }

        void Update()
        {
            if (_patchUpdater == null || _completed) return;

            if (_patchUpdater.IsUpdating)
            {
                progressText.text = _patchUpdater.GetProgressText();
                progress.DOFillAmount(_patchUpdater.Progress, 0.2f);
            }
            else if (_patchUpdater.UpdateComplete)
            {
                progressText.text = "更新完成!";
                progress.DOFillAmount(1f, 0.2f);
                _completed = true;
            }
            else if (_patchUpdater.UpdateFailed)
            {
                progressText.text = $"更新失败:\n{_patchUpdater.ErrorMessage}";
            }
        }

        public void EnterScene()
        {
            var uiManager = GlobalInitializer.Instance.GetModule<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[UIExample] UIManager模块未找到");
                return;
            }

            uiManager.OpenPanel(PanelType.Loading);

            var loadingScreen = uiManager.GetPanelComponent<LoadingScreen>(PanelType.Loading);
            if (loadingScreen != null)
                loadingScreen.LoadScene(targetSceneName);
            else
                Debug.LogError("[UIExample] LoadingScreen组件未找到");
        }
    }
}
