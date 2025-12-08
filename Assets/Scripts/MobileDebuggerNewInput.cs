using UnityEngine;
using UnityEngine.Rendering; // 用于访问 DebugManager
using UnityEngine.InputSystem; // New Input System 核心
using UnityEngine.InputSystem.EnhancedTouch; // 用于更方便的触摸检测
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch; // 消除与 UnityEngine.Touch 的歧义

public class MobileDebuggerNewInput : MonoBehaviour
{
    private void OnEnable()
    {
        // 关键：必须启用 EnhancedTouchSupport，否则 Touch.activeTouches 不会更新
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        // 养成好习惯，禁用时关闭支持
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        // --- PC / 编辑器调试 (按 F3) ---
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ToggleRenderingDebugger();
        }

        // --- 移动端逻辑 (三指点击) ---
        // Touch.activeTouches.Count 类似旧版的 Input.touchCount
        if (Touch.activeTouches.Count == 3)
        {
            // 遍历当前活跃的触摸，只要有一根手指是"这一帧刚按下" (Began)，就触发
            // 这样可以防止手一直按在屏幕上时每帧都切换
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    ToggleRenderingDebugger();
                    break; // 触发一次后跳出，避免重复
                }
            }
        }

        if (Keyboard.current != null)
        {
            DetailManager detail = FindObjectOfType<DetailManager>();
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                if (detail.maxDrawLayer >= 30)
                {
                    detail.maxDrawLayer = 0;
                }
                detail.maxDrawLayer += 1;
                detail.Cleanup();
                detail.Initialize();
            }
            else if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                detail.maxDistance += 100;
            }
            else if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                detail.fadeStart += 100;
            }
            else if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                detail.grassDensity = 0.2f;
                detail.Cleanup();
                detail.Initialize();
            }
        }
    }

    public void ToggleRenderingDebugger()
    {
        // 获取 DebugManager 单例
        var manager = DebugManager.instance;
        if (manager == null) return;

        // 切换 UI 显示状态
        manager.enableRuntimeUI = !manager.enableRuntimeUI;

        if (manager.enableRuntimeUI)
        {
            Debug.Log("[MobileDebugger] Rendering Debugger Opened");
        }
    }
}