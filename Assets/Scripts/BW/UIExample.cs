// UIExample.cs (UI使用示例)

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTools
{
    public class UIExample : MonoBehaviour
    {
        public PatchUpdaterMono patchUpdater;
        public Button updateButton;
        public Text progressText;
        public Image progress;

        void Start()
        {
            updateButton.onClick.AddListener(OnUpdateButtonClick);
        }

        void Update()
        {
            if (patchUpdater.IsUpdating)
            {
                progressText.text = patchUpdater.GetProgressText();
                progress.DOFillAmount(patchUpdater.Progress, 0.2f);
                updateButton.interactable = false;
            }
            else
            {
                updateButton.interactable = true;
                
                if (patchUpdater.UpdateComplete)
                {
                    progressText.text = "更新完成!";
                    progress.DOFillAmount(1, 0.2f);
                }
                else if (patchUpdater.UpdateFailed)
                {
                    progressText.text = $"更新失败:\n{patchUpdater.ErrorMessage}";
                }
            }
        }

        void OnUpdateButtonClick()
        {
            var playMode = GlobalInitializer.Instance.GetPlayMode();
            if(playMode != EPlayMode.HostPlayMode)
            {
                Debug.Log("当前非热更模式，无需更新资源");
                return;
            }
            patchUpdater.StartUpdate();
        }
    }
}