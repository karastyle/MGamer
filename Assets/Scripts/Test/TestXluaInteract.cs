using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;


namespace Test
{
    // 建议放在一个单独的非 MonoBehaviour 静态类中
    public static class XLuaGenConfig
    {
        [CSharpCallLua]
        public static List<Type> CSharpCallLuaList = new List<Type>() {
            typeof(Action<int, PointerEventData>),
            typeof(Action<PointerEventData>),
            typeof(Action),
        };
    }
    
    public struct ClickEventData {
        public int eventId;
        public float x;
        public float y;
    }
    
    [LuaCallCSharp]
    public class TestXluaInteract : MonoBehaviour, IPointerClickHandler
    {
        // 挂在 Button 上的脚本：允许 Lua 端绑定一个回调（XLua 的 LuaFunction），
        // 点击 Button 时会执行该回调。
        // 为了在没有 XLua 定义的环境下也能编译和测试，使用条件编译：
        // 当编译符号 XLUA 存在时使用 XLua.LuaFunction，否则使用 System.Action 作回退。

        private Button _button;

        private LuaFunction _luaCallback;

        private Action _luaActionCallback;
        
        private Action<PointerEventData> _luaActionWithParamsCallback;

        private static int _idCounter = 0; // 全局计数器
    
        [SerializeField] private int globalEventId = 0; // 默认是0，表示未分配

        public int ClickEventId => globalEventId;
        
        private static readonly ClickEventData[] _buffer = new ClickEventData[1000]; 
        private static int _count = 0;
        
        // 当 Lua 调用这个方法时，如果没有 ID，就分一个
        public int AddClickListener() {
            if (globalEventId <= 0) {
                _idCounter++;
                globalEventId = _idCounter;
            }
            return globalEventId;
        }

        void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }
        }

        // 这个方法是接口要求的，它带有 PointerEventData 参数
        public void OnPointerClick(PointerEventData eventData)
        {
            // 这里你可以拿到所有点击细节
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out localPos
            );

            Debug.Log($"点击位置: {eventData.position}, 相对坐标: {localPos}");
        
            if (_luaActionWithParamsCallback != null)
            {
                _luaActionWithParamsCallback.Invoke(eventData);
            }

            if (globalEventId > 0)
            {
                PushEvent(globalEventId, localPos.x, localPos.y);
            }
        }
        
        public static void PushEvent(int id, float x, float y) {
            if (_count < _buffer.Length) {
                _buffer[_count].eventId = id;
                _buffer[_count].x = x;
                _buffer[_count].y = y;
                _count++;
            }
        }

        // 2. 提供获取原始数据的方法，而不是返回 List 对象
        public static int GetEvents(out ClickEventData[] data) {
            data = _buffer;
            return _count;
        }

        public static void Clear() => _count = 0;
        
        /// <summary>
        /// 按钮点击时调用（由 Button 的 onClick 调用）
        /// </summary>
        public void OnClick()
        {
            if (_luaCallback != null)
            {
                // 不传参数，直接调用；如果需要传参可以修改为 Call(args...)
                _luaCallback.Call();
            }

            if (_luaActionCallback != null)
            {
                _luaActionCallback.Invoke();
            }
            
        }

        /// <summary>
        /// 由 Lua 侧传入一个 LuaFunction 用来在点击时回调。
        /// 在绑定新回调前会释放之前的 LuaFunction 引用。
        /// </summary>
        public void Bind(LuaFunction fn)
        {
            if (_luaCallback != null)
            {
                _luaCallback.Dispose();
            }
            _luaCallback = fn;
        }
        
        public void BindAction(Action callback)
        {
            _luaActionCallback = callback;
        }

        public void BindParamsAction(Action<PointerEventData> callback)
        {
            _luaActionWithParamsCallback = callback;
        }
        
        public void BindParamsAction_Default(Action<PointerEventData> callback, bool clear = true)
        {
            _luaActionWithParamsCallback = callback;
        }

        /// <summary>
        /// 解绑回调并释放资源（若适用）。
        /// </summary>
        public void Unbind()
        {
            if (_luaCallback != null)
            {
                _luaCallback.Dispose();
                _luaCallback = null;
            }
            
            _luaActionCallback = null;
            _luaActionWithParamsCallback = null;
        }

        void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
            Unbind();
        }
    }
}