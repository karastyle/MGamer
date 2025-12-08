using UnityEngine;

[DisallowMultipleComponent]
public class DontDestroyOnLoadObject : MonoBehaviour
{
    private void Awake()
    {
        // 让当前对象在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
    }
}