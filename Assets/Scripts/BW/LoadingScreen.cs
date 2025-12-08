using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyTools;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public Image progressBar;
    public Text statusText;

    private string _targetSceneName;

    private SceneHandle _sceneHandle;

    private void Awake()
    {
        _targetSceneName = GlobalInitializer.Instance.loadingSceneName;
        StartCoroutine(LoadSceneCoroutine());
    }

    private IEnumerator LoadSceneCoroutine()
    {
        UpdateUI("初始化加载...", 0);
        yield return null;

        // 开始加载场景
        _sceneHandle = EasyAsset.Instance.LoadSceneAsync(_targetSceneName);
        
        // 等待场景加载完成，实时更新进度
        while (!_sceneHandle.IsDone)
        {
            float progress = Mathf.Clamp01(_sceneHandle.Progress);
            UpdateUI($"场景加载中... {(int)(progress * 100)}%", progress);
            yield return null;
        }

        // 检查加载是否成功
        if (_sceneHandle.Status == EProviderStatus.Succeed)
        {
            UpdateUI("加载完成！", 1f);
            yield return new WaitForSeconds(0.3f);
            
            // 激活场景
            if (_sceneHandle.ActivateScene())
            {
                Debug.Log($"场景已激活: {_targetSceneName}");
            }
            else
            {
                Debug.LogWarning($"场景激活失败: {_targetSceneName}");
            }
            
            // 关闭Loading界面
            var uiManager = GlobalInitializer.Instance.GetModule<UIManager>();
            if ( uiManager != null)
            {
                uiManager.ClosePanel(PanelType.Loading);
            }
        }
        else
        {
            UpdateUI($"加载失败: {_sceneHandle.LastError}", 0);
            Debug.LogError($"场景加载失败: {_sceneHandle.LastError}");
        }
    }

    private void UpdateUI(string message, float progress)
    {
        if (statusText != null) statusText.text = message;
        if (progressBar != null) progressBar.fillAmount = progress;
    }

    private void OnDestroy()
    {
        _sceneHandle?.Release();
    }
}