Shader "Hidden/TerrainBake/Normal"
{
    Properties
    {
        _Control0 ("Control 0 (RGBA)", 2D) = "red" {}
        _Control1 ("Control 1 (RGBA)", 2D) = "black" {}
        
        _Normal0 ("Layer 0 (Normal)", 2D) = "bump" {}
        _Normal1 ("Layer 1 (Normal)", 2D) = "bump" {}
        _Normal2 ("Layer 2 (Normal)", 2D) = "bump" {}
        _Normal3 ("Layer 3 (Normal)", 2D) = "bump" {}
        _Normal4 ("Layer 4 (Normal)", 2D) = "bump" {}
        _Normal5 ("Layer 5 (Normal)", 2D) = "bump" {}
        _Normal6 ("Layer 6 (Normal)", 2D) = "bump" {}
        _Normal7 ("Layer 7 (Normal)", 2D) = "bump" {}
        
        _NormalScale0 ("Normal Scale 0", Float) = 1.0
        _NormalScale1 ("Normal Scale 1", Float) = 1.0
        _NormalScale2 ("Normal Scale 2", Float) = 1.0
        _NormalScale3 ("Normal Scale 3", Float) = 1.0
        _NormalScale4 ("Normal Scale 4", Float) = 1.0
        _NormalScale5 ("Normal Scale 5", Float) = 1.0
        _NormalScale6 ("Normal Scale 6", Float) = 1.0
        _NormalScale7 ("Normal Scale 7", Float) = 1.0
        
        _UVOffset ("UV Offset", Vector) = (0, 0, 0, 0)
        _UVScale ("UV Scale", Vector) = (1, 1, 1, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
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
            
            sampler2D _Normal0;
            sampler2D _Normal1;
            sampler2D _Normal2;
            sampler2D _Normal3;
            sampler2D _Normal4;
            sampler2D _Normal5;
            sampler2D _Normal6;
            sampler2D _Normal7;
            
            float _NormalScale0;
            float _NormalScale1;
            float _NormalScale2;
            float _NormalScale3;
            float _NormalScale4;
            float _NormalScale5;
            float _NormalScale6;
            float _NormalScale7;
            
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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Unity标准的UnpackNormal函数
            inline fixed3 UnpackNormalWithScale_Custom(fixed4 packednormal, float scale)
            {
                fixed3 normal;
                normal.xy = (packednormal.ag * 2.0 - 1.0) * scale;
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                return normal;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算实际的地形UV坐标
                float2 terrainUV = _UVOffset.xy + i.uv * _UVScale.xy;
                
                // 采样控制纹理
                fixed4 control0 = tex2D(_Control0, terrainUV);
                fixed4 control1 = tex2D(_Control1, terrainUV);
                
                // 累积法线（切线空间）
                fixed3 mixedNormal = fixed3(0, 0, 0);
                
                // Layer 0-3
                if (control0.r > 0.001)
                {
                    float2 splatUV0 = terrainUV * _Splat0_ST.xy + _Splat0_ST.zw;
                    fixed4 normalTex0 = tex2D(_Normal0, splatUV0);
                    fixed3 normal0 = UnpackNormalWithScale_Custom(normalTex0, _NormalScale0);
                    mixedNormal += normal0 * control0.r;
                }
                
                if (control0.g > 0.001)
                {
                    float2 splatUV1 = terrainUV * _Splat1_ST.xy + _Splat1_ST.zw;
                    fixed4 normalTex1 = tex2D(_Normal1, splatUV1);
                    fixed3 normal1 = UnpackNormalWithScale_Custom(normalTex1, _NormalScale1);
                    mixedNormal += normal1 * control0.g;
                }
                
                if (control0.b > 0.001)
                {
                    float2 splatUV2 = terrainUV * _Splat2_ST.xy + _Splat2_ST.zw;
                    fixed4 normalTex2 = tex2D(_Normal2, splatUV2);
                    fixed3 normal2 = UnpackNormalWithScale_Custom(normalTex2, _NormalScale2);
                    mixedNormal += normal2 * control0.b;
                }
                
                if (control0.a > 0.001)
                {
                    float2 splatUV3 = terrainUV * _Splat3_ST.xy + _Splat3_ST.zw;
                    fixed4 normalTex3 = tex2D(_Normal3, splatUV3);
                    fixed3 normal3 = UnpackNormalWithScale_Custom(normalTex3, _NormalScale3);
                    mixedNormal += normal3 * control0.a;
                }
                
                // Layer 4-7
                if (control1.r > 0.001)
                {
                    float2 splatUV4 = terrainUV * _Splat4_ST.xy + _Splat4_ST.zw;
                    fixed4 normalTex4 = tex2D(_Normal4, splatUV4);
                    fixed3 normal4 = UnpackNormalWithScale_Custom(normalTex4, _NormalScale4);
                    mixedNormal += normal4 * control1.r;
                }
                
                if (control1.g > 0.001)
                {
                    float2 splatUV5 = terrainUV * _Splat5_ST.xy + _Splat5_ST.zw;
                    fixed4 normalTex5 = tex2D(_Normal5, splatUV5);
                    fixed3 normal5 = UnpackNormalWithScale_Custom(normalTex5, _NormalScale5);
                    mixedNormal += normal5 * control1.g;
                }
                
                if (control1.b > 0.001)
                {
                    float2 splatUV6 = terrainUV * _Splat6_ST.xy + _Splat6_ST.zw;
                    fixed4 normalTex6 = tex2D(_Normal6, splatUV6);
                    fixed3 normal6 = UnpackNormalWithScale_Custom(normalTex6, _NormalScale6);
                    mixedNormal += normal6 * control1.b;
                }
                
                if (control1.a > 0.001)
                {
                    float2 splatUV7 = terrainUV * _Splat7_ST.xy + _Splat7_ST.zw;
                    fixed4 normalTex7 = tex2D(_Normal7, splatUV7);
                    fixed3 normal7 = UnpackNormalWithScale_Custom(normalTex7, _NormalScale7);
                    mixedNormal += normal7 * control1.a;
                }
                
                // 如果没有任何控制权重，使用默认法线
                float totalWeight = control0.r + control0.g + control0.b + control0.a + 
                                   control1.r + control1.g + control1.b + control1.a;
                
                if (totalWeight < 0.001)
                {
                    mixedNormal = fixed3(0, 0, 1);
                }
                else
                {
                    // 归一化
                    mixedNormal = normalize(mixedNormal);
                }
                
                // 转换回颜色空间 (0-1)
                fixed4 output;
                output.rgb = mixedNormal * 0.5 + 0.5;
                output.a = 1.0;
                
                return output;
            }
            ENDCG
        }
    }
}