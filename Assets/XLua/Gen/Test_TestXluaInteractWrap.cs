#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class TestTestXluaInteractWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(Test.TestXluaInteract);
			Utils.BeginObjectRegister(type, L, translator, 0, 8, 1, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AddClickListener", _m_AddClickListener);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerClick", _m_OnPointerClick);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnClick", _m_OnClick);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Bind", _m_Bind);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BindAction", _m_BindAction);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BindParamsAction", _m_BindParamsAction);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BindParamsAction_Default", _m_BindParamsAction_Default);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Unbind", _m_Unbind);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "ClickEventId", _g_get_ClickEventId);
            
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 4, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "PushEvent", _m_PushEvent_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetEvents", _m_GetEvents_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Clear", _m_Clear_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new Test.TestXluaInteract();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to Test.TestXluaInteract constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddClickListener(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        var gen_ret = gen_to_be_invoked.AddClickListener(  );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnPointerClick(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.EventSystems.PointerEventData _eventData = (UnityEngine.EventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.EventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnPointerClick( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PushEvent_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _id = LuaAPI.xlua_tointeger(L, 1);
                    float _x = (float)LuaAPI.lua_tonumber(L, 2);
                    float _y = (float)LuaAPI.lua_tonumber(L, 3);
                    
                    Test.TestXluaInteract.PushEvent( _id, _x, _y );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetEvents_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    Test.ClickEventData[] _data;
                    
                        var gen_ret = Test.TestXluaInteract.GetEvents( out _data );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    translator.Push(L, _data);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Clear_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    Test.TestXluaInteract.Clear(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnClick(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.OnClick(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Bind(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    XLua.LuaFunction _fn = (XLua.LuaFunction)translator.GetObject(L, 2, typeof(XLua.LuaFunction));
                    
                    gen_to_be_invoked.Bind( _fn );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BindAction(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Action _callback = translator.GetDelegate<System.Action>(L, 2);
                    
                    gen_to_be_invoked.BindAction( _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BindParamsAction(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Action<UnityEngine.EventSystems.PointerEventData> _callback = translator.GetDelegate<System.Action<UnityEngine.EventSystems.PointerEventData>>(L, 2);
                    
                    gen_to_be_invoked.BindParamsAction( _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BindParamsAction_Default(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& translator.Assignable<System.Action<UnityEngine.EventSystems.PointerEventData>>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    System.Action<UnityEngine.EventSystems.PointerEventData> _callback = translator.GetDelegate<System.Action<UnityEngine.EventSystems.PointerEventData>>(L, 2);
                    bool _clear = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.BindParamsAction_Default( _callback, _clear );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<System.Action<UnityEngine.EventSystems.PointerEventData>>(L, 2)) 
                {
                    System.Action<UnityEngine.EventSystems.PointerEventData> _callback = translator.GetDelegate<System.Action<UnityEngine.EventSystems.PointerEventData>>(L, 2);
                    
                    gen_to_be_invoked.BindParamsAction_Default( _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to Test.TestXluaInteract.BindParamsAction_Default!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Unbind(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Unbind(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ClickEventId(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                Test.TestXluaInteract gen_to_be_invoked = (Test.TestXluaInteract)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ClickEventId);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
		
		
		
		
    }
}
