Shader "Hidden/TerrainBake/Metallic"
{
    Properties
    {
        _Control0 ("Control 0 (RGBA)", 2D) = "red" {}
        _Control1 ("Control 1 (RGBA)", 2D) = "black" {}
        
        _Mask0 ("Layer 0 (Mask)", 2D) = "white" {}
        _Mask1 ("Layer 1 (Mask)", 2D) = "white" {}
        _Mask2 ("Layer 2 (Mask)", 2D) = "white" {}
        _Mask3 ("Layer 3 (Mask)", 2D) = "white" {}
        _Mask4 ("Layer 4 (Mask)", 2D) = "white" {}
        _Mask5 ("Layer 5 (Mask)", 2D) = "white" {}
        _Mask6 ("Layer 6 (Mask)", 2D) = "white" {}
        _Mask7 ("Layer 7 (Mask)", 2D) = "white" {}
        
        _Metallic0 ("Metallic 0", Float) = 0.0
        _Metallic1 ("Metallic 1", Float) = 0.0
        _Metallic2 ("Metallic 2", Float) = 0.0
        _Metallic3 ("Metallic 3", Float) = 0.0
        _Metallic4 ("Metallic 4", Float) = 0.0
        _Metallic5 ("Metallic 5", Float) = 0.0
        _Metallic6 ("Metallic 6", Float) = 0.0
        _Metallic7 ("Metallic 7", Float) = 0.0
        
        _Smoothness0 ("Smoothness 0", Float) = 0.5
        _Smoothness1 ("Smoothness 1", Float) = 0.5
        _Smoothness2 ("Smoothness 2", Float) = 0.5
        _Smoothness3 ("Smoothness 3", Float) = 0.5
        _Smoothness4 ("Smoothness 4", Float) = 0.5
        _Smoothness5 ("Smoothness 5", Float) = 0.5
        _Smoothness6 ("Smoothness 6", Float) = 0.5
        _Smoothness7 ("Smoothness 7", Float) = 0.5
        
        _UVOffset ("UV Offset", Vector) = (0, 0, 0, 0)
        _UVScale ("UV Scale", Vector) = (1, 1, 1, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _Control0;
            sampler2D _Control1;
            
            sampler2D _Mask0;
            sampler2D _Mask1;
            sampler2D _Mask2;
            sampler2D _Mask3;
            sampler2D _Mask4;
            sampler2D _Mask5;
            sampler2D _Mask6;
            sampler2D _Mask7;
            
            float _Metallic0;
            float _Metallic1;
            float _Metallic2;
            float _Metallic3;
            float _Metallic4;
            float _Metallic5;
            float _Metallic6;
            float _Metallic7;
            
            float _Smoothness0;
            float _Smoothness1;
            float _Smoothness2;
            float _Smoothness3;
            float _Smoothness4;
            float _Smoothness5;
            float _Smoothness6;
            float _Smoothness7;
            
            float4 _Splat0_ST;
            float4 _Splat1_ST;
            float4 _Splat2_ST;
            float4 _Splat3_ST;
            float4 _Splat4_ST;
            float4 _Splat5_ST;
            float4 _Splat6_ST;
            float4 _Splat7_ST;
            
            // Channel Remapping 参数
            float4 _MaskRemapMin0;
            float4 _MaskRemapMin1;
            float4 _MaskRemapMin2;
            float4 _MaskRemapMin3;
            float4 _MaskRemapMin4;
            float4 _MaskRemapMin5;
            float4 _MaskRemapMin6;
            float4 _MaskRemapMin7;
            
            float4 _MaskRemapMax0;
            float4 _MaskRemapMax1;
            float4 _MaskRemapMax2;
            float4 _MaskRemapMax3;
            float4 _MaskRemapMax4;
            float4 _MaskRemapMax5;
            float4 _MaskRemapMax6;
            float4 _MaskRemapMax7;
            
            float4 _UVOffset;
            float4 _UVScale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv * 2.0 - 1.0, 0.0, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -o.vertex.y;
                #endif
                o.uv = v.uv;
                return o;
            }
            
            // 应用Channel Remapping
            float ApplyRemap(float value, float minVal, float maxVal)
            {
                return value * (maxVal - minVal) + minVal;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 terrainUV = _UVOffset.xy + i.uv * _UVScale.xy;
                
                fixed4 control0 = tex2D(_Control0, terrainUV);
                fixed4 control1 = tex2D(_Control1, terrainUV);
                
                float mixedMetallic = 0.0;
                float mixedSmoothness = 0.0;
                
                // Layer 0
                if (control0.r > 0.001)
                {
                    float2 splatUV0 = terrainUV * _Splat0_ST.xy + _Splat0_ST.zw;
                    fixed4 mask0 = tex2D(_Mask0, splatUV0);
                    
                    // 应用Channel Remapping
                    float metallic0 = ApplyRemap(mask0.r, _MaskRemapMin0.x, _MaskRemapMax0.x);
                    float smoothness0 = ApplyRemap(mask0.a, _MaskRemapMin0.w, _MaskRemapMax0.w);
                    
                    // 如果Mask贴图无效，使用Layer的默认值
                    if (mask0.r < 0.001 && mask0.a < 0.001)
                    {
                        metallic0 = _Metallic0;
                        smoothness0 = _Smoothness0;
                    }
                    
                    mixedMetallic += metallic0 * control0.r;
                    mixedSmoothness += smoothness0 * control0.r;
                }
                
                // Layer 1
                if (control0.g > 0.001)
                {
                    float2 splatUV1 = terrainUV * _Splat1_ST.xy + _Splat1_ST.zw;
                    fixed4 mask1 = tex2D(_Mask1, splatUV1);
                    
                    float metallic1 = ApplyRemap(mask1.r, _MaskRemapMin1.x, _MaskRemapMax1.x);
                    float smoothness1 = ApplyRemap(mask1.a, _MaskRemapMin1.w, _MaskRemapMax1.w);
                    
                    if (mask1.r < 0.001 && mask1.a < 0.001)
                    {
                        metallic1 = _Metallic1;
                        smoothness1 = _Smoothness1;
                    }
                    
                    mixedMetallic += metallic1 * control0.g;
                    mixedSmoothness += smoothness1 * control0.g;
                }
                
                // Layer 2
                if (control0.b > 0.001)
                {
                    float2 splatUV2 = terrainUV * _Splat2_ST.xy + _Splat2_ST.zw;
                    fixed4 mask2 = tex2D(_Mask2, splatUV2);
                    
                    float metallic2 = ApplyRemap(mask2.r, _MaskRemapMin2.x, _MaskRemapMax2.x);
                    float smoothness2 = ApplyRemap(mask2.a, _MaskRemapMin2.w, _MaskRemapMax2.w);
                    
                    if (mask2.r < 0.001 && mask2.a < 0.001)
                    {
                        metallic2 = _Metallic2;
                        smoothness2 = _Smoothness2;
                    }
                    
                    mixedMetallic += metallic2 * control0.b;
                    mixedSmoothness += smoothness2 * control0.b;
                }
                
                // Layer 3
                if (control0.a > 0.001)
                {
                    float2 splatUV3 = terrainUV * _Splat3_ST.xy + _Splat3_ST.zw;
                    fixed4 mask3 = tex2D(_Mask3, splatUV3);
                    
                    float metallic3 = ApplyRemap(mask3.r, _MaskRemapMin3.x, _MaskRemapMax3.x);
                    float smoothness3 = ApplyRemap(mask3.a, _MaskRemapMin3.w, _MaskRemapMax3.w);
                    
                    if (mask3.r < 0.001 && mask3.a < 0.001)
                    {
                        metallic3 = _Metallic3;
                        smoothness3 = _Smoothness3;
                    }
                    
                    mixedMetallic += metallic3 * control0.a;
                    mixedSmoothness += smoothness3 * control0.a;
                }
                
                // Layer 4
                if (control1.r > 0.001)
                {
                    float2 splatUV4 = terrainUV * _Splat4_ST.xy + _Splat4_ST.zw;
                    fixed4 mask4 = tex2D(_Mask4, splatUV4);
                    
                    float metallic4 = ApplyRemap(mask4.r, _MaskRemapMin4.x, _MaskRemapMax4.x);
                    float smoothness4 = ApplyRemap(mask4.a, _MaskRemapMin4.w, _MaskRemapMax4.w);
                    
                    if (mask4.r < 0.001 && mask4.a < 0.001)
                    {
                        metallic4 = _Metallic4;
                        smoothness4 = _Smoothness4;
                    }
                    
                    mixedMetallic += metallic4 * control1.r;
                    mixedSmoothness += smoothness4 * control1.r;
                }
                
                // Layer 5
                if (control1.g > 0.001)
                {
                    float2 splatUV5 = terrainUV * _Splat5_ST.xy + _Splat5_ST.zw;
                    fixed4 mask5 = tex2D(_Mask5, splatUV5);
                    
                    float metallic5 = ApplyRemap(mask5.r, _MaskRemapMin5.x, _MaskRemapMax5.x);
                    float smoothness5 = ApplyRemap(mask5.a, _MaskRemapMin5.w, _MaskRemapMax5.w);
                    
                    if (mask5.r < 0.001 && mask5.a < 0.001)
                    {
                        metallic5 = _Metallic5;
                        smoothness5 = _Smoothness5;
                    }
                    
                    mixedMetallic += metallic5 * control1.g;
                    mixedSmoothness += smoothness5 * control1.g;
                }
                
                // Layer 6
                if (control1.b > 0.001)
                {
                    float2 splatUV6 = terrainUV * _Splat6_ST.xy + _Splat6_ST.zw;
                    fixed4 mask6 = tex2D(_Mask6, splatUV6);
                    
                    float metallic6 = ApplyRemap(mask6.r, _MaskRemapMin6.x, _MaskRemapMax6.x);
                    float smoothness6 = ApplyRemap(mask6.a, _MaskRemapMin6.w, _MaskRemapMax6.w);
                    
                    if (mask6.r < 0.001 && mask6.a < 0.001)
                    {
                        metallic6 = _Metallic6;
                        smoothness6 = _Smoothness6;
                    }
                    
                    mixedMetallic += metallic6 * control1.b;
                    mixedSmoothness += smoothness6 * control1.b;
                }
                
                // Layer 7
                if (control1.a > 0.001)
                {
                    float2 splatUV7 = terrainUV * _Splat7_ST.xy + _Splat7_ST.zw;
                    fixed4 mask7 = tex2D(_Mask7, splatUV7);
                    
                    float metallic7 = ApplyRemap(mask7.r, _MaskRemapMin7.x, _MaskRemapMax7.x);
                    float smoothness7 = ApplyRemap(mask7.a, _MaskRemapMin7.w, _MaskRemapMax7.w);
                    
                    if (mask7.r < 0.001 && mask7.a < 0.001)
                    {
                        metallic7 = _Metallic7;
                        smoothness7 = _Smoothness7;
                    }
                    
                    mixedMetallic += metallic7 * control1.a;
                    mixedSmoothness += smoothness7 * control1.a;
                }
                
                // 输出格式：RGB=Metallic, A=Smoothness
                return fixed4(mixedMetallic, mixedMetallic, mixedMetallic, mixedSmoothness);
            }
            ENDCG
        }
    }
}