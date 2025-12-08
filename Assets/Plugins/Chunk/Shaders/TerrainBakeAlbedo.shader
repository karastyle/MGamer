Shader "Hidden/TerrainBake/Albedo"
{
    Properties
    {
        _Control0 ("Control 0 (RGBA)", 2D) = "red" {}
        _Control1 ("Control 1 (RGBA)", 2D) = "black" {}
        
        _Splat0 ("Layer 0 (Albedo)", 2D) = "white" {}
        _Splat1 ("Layer 1 (Albedo)", 2D) = "white" {}
        _Splat2 ("Layer 2 (Albedo)", 2D) = "white" {}
        _Splat3 ("Layer 3 (Albedo)", 2D) = "white" {}
        _Splat4 ("Layer 4 (Albedo)", 2D) = "white" {}
        _Splat5 ("Layer 5 (Albedo)", 2D) = "white" {}
        _Splat6 ("Layer 6 (Albedo)", 2D) = "white" {}
        _Splat7 ("Layer 7 (Albedo)", 2D) = "white" {}
        
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
            
            sampler2D _Splat0;
            sampler2D _Splat1;
            sampler2D _Splat2;
            sampler2D _Splat3;
            sampler2D _Splat4;
            sampler2D _Splat5;
            sampler2D _Splat6;
            sampler2D _Splat7;
            
            float4 _Splat0_ST;
            float4 _Splat1_ST;
            float4 _Splat2_ST;
            float4 _Splat3_ST;
            float4 _Splat4_ST;
            float4 _Splat5_ST;
            float4 _Splat6_ST;
            float4 _Splat7_ST;
            
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
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算实际的地形UV坐标
                float2 terrainUV = _UVOffset.xy + i.uv * _UVScale.xy;
                
                // 采样控制纹理
                fixed4 control0 = tex2D(_Control0, terrainUV);
                fixed4 control1 = tex2D(_Control1, terrainUV);
                
                // 调试：如果没有控制权重，返回洋红色
                float totalWeight = control0.r + control0.g + control0.b + control0.a;
                
                if (totalWeight < 0.001)
                {
                    // 没有任何层，返回洋红色以便调试
                    return fixed4(1, 0, 1, 1);
                }
                
                // 混合所有层
                fixed4 mixedAlbedo = fixed4(0, 0, 0, 0);
                
                // Layer 0
                if (control0.r > 0.001)
                {
                    float2 splatUV0 = terrainUV * _Splat0_ST.xy + _Splat0_ST.zw;
                    fixed4 color0 = tex2D(_Splat0, splatUV0);
                    mixedAlbedo += color0 * control0.r;
                }
                
                // Layer 1
                if (control0.g > 0.001)
                {
                    float2 splatUV1 = terrainUV * _Splat1_ST.xy + _Splat1_ST.zw;
                    fixed4 color1 = tex2D(_Splat1, splatUV1);
                    mixedAlbedo += color1 * control0.g;
                }
                
                // Layer 2
                if (control0.b > 0.001)
                {
                    float2 splatUV2 = terrainUV * _Splat2_ST.xy + _Splat2_ST.zw;
                    fixed4 color2 = tex2D(_Splat2, splatUV2);
                    mixedAlbedo += color2 * control0.b;
                }
                
                // Layer 3
                if (control0.a > 0.001)
                {
                    float2 splatUV3 = terrainUV * _Splat3_ST.xy + _Splat3_ST.zw;
                    fixed4 color3 = tex2D(_Splat3, splatUV3);
                    mixedAlbedo += color3 * control0.a;
                }
                
                // Layer 4-7 使用Control1...
                if (control1.r > 0.001)
                {
                    float2 splatUV4 = terrainUV * _Splat4_ST.xy + _Splat4_ST.zw;
                    fixed4 color4 = tex2D(_Splat4, splatUV4);
                    mixedAlbedo += color4 * control1.r;
                }
                
                if (control1.g > 0.001)
                {
                    float2 splatUV5 = terrainUV * _Splat5_ST.xy + _Splat5_ST.zw;
                    fixed4 color5 = tex2D(_Splat5, splatUV5);
                    mixedAlbedo += color5 * control1.g;
                }
                
                if (control1.b > 0.001)
                {
                    float2 splatUV6 = terrainUV * _Splat6_ST.xy + _Splat6_ST.zw;
                    fixed4 color6 = tex2D(_Splat6, splatUV6);
                    mixedAlbedo += color6 * control1.b;
                }
                
                if (control1.a > 0.001)
                {
                    float2 splatUV7 = terrainUV * _Splat7_ST.xy + _Splat7_ST.zw;
                    fixed4 color7 = tex2D(_Splat7, splatUV7);
                    mixedAlbedo += color7 * control1.a;
                }
                
                mixedAlbedo.a = 1.0;
                return mixedAlbedo;
            }
            ENDCG
        }
    }
}