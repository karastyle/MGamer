using System.Collections;
using UnityEngine;

// 定义界面的枚举
public enum PanelType
{
    Update,
    Loading
}

public class UIManager : MonoBehaviour
{

    [Header("Panels")]
    public GameObject updateView;
    public GameObject loadingView;

    public IEnumerator Initialize()
    {
        yield return null;
    }

    /// <summary>
    /// 打开指定界面
    /// </summary>
    public void OpenPanel(PanelType type)
    {
        GameObject panel = GetPanel(type);
        if (panel != null) panel.SetActive(true);
    }

    /// <summary>
    /// 关闭指定界面
    /// </summary>
    public void ClosePanel(PanelType type)
    {
        GameObject panel = GetPanel(type);
        if (panel != null) panel.SetActive(false);
    }

    /// <summary>
    /// 获取面板上的指定组件
    /// </summary>
    public T GetPanelComponent<T>(PanelType type) where T : Component
    {
        GameObject panel = GetPanel(type);
        return panel != null ? panel.GetComponent<T>() : null;
    }

    /// <summary>
    /// 内部帮助方法：将枚举映射到 GameObject
    /// </summary>
    private GameObject GetPanel(PanelType type)
    {
        switch (type)
        {
            case PanelType.Update: return updateView;
            case PanelType.Loading: return loadingView;
            default: return null;
        }
    }
}