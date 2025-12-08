using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("UI组件")]
    public Text displayText;
    
    [Header("更新设置")]
    [SerializeField] private float updateInterval = 0.5f;
    
    private float deltaTime = 0f;
    private float timer = 0f;
    private int frameCount = 0;
    private float fps = 0f;
    
    void Update()
    {
        deltaTime += Time.unscaledDeltaTime;
        frameCount++;
        timer += Time.unscaledDeltaTime;
        
        if (timer >= updateInterval)
        {
            fps = frameCount / deltaTime;
            
            // 显示信息
            displayText.text = $"FPS: {fps:F1}\n";
            
            deltaTime = 0f;
            frameCount = 0;
            timer = 0f;
        }
    }
}