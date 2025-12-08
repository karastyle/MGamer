Shader "Hidden/TerrainBake/Occlusion"
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
            
            float4 _Splat0_ST;
            float4 _Splat1_ST;
            float4 _Splat2_ST;
            float4 _Splat3_ST;
            float4 _Splat4_ST;
            float4 _Splat5_ST;
            float4 _Splat6_ST;
            float4 _Splat7_ST;
            
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
            
            float ApplyRemap(float value, float minVal, float maxVal)
            {
                return value * (maxVal - minVal) + minVal;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 terrainUV = _UVOffset.xy + i.uv * _UVScale.xy;
                
                fixed4 control0 = tex2D(_Control0, terrainUV);
                fixed4 control1 = tex2D(_Control1, terrainUV);
                
                float mixedOcclusion = 1.0;
                
                // Layer 0-3
                if (control0.r > 0.001)
                {
                    float2 splatUV0 = terrainUV * _Splat0_ST.xy + _Splat0_ST.zw;
                    fixed4 mask0 = tex2D(_Mask0, splatUV0);
                    float ao0 = ApplyRemap(mask0.g, _MaskRemapMin0.y, _MaskRemapMax0.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao0, control0.r));
                }
                
                if (control0.g > 0.001)
                {
                    float2 splatUV1 = terrainUV * _Splat1_ST.xy + _Splat1_ST.zw;
                    fixed4 mask1 = tex2D(_Mask1, splatUV1);
                    float ao1 = ApplyRemap(mask1.g, _MaskRemapMin1.y, _MaskRemapMax1.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao1, control0.g));
                }
                
                if (control0.b > 0.001)
                {
                    float2 splatUV2 = terrainUV * _Splat2_ST.xy + _Splat2_ST.zw;
                    fixed4 mask2 = tex2D(_Mask2, splatUV2);
                    float ao2 = ApplyRemap(mask2.g, _MaskRemapMin2.y, _MaskRemapMax2.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao2, control0.b));
                }
                
                if (control0.a > 0.001)
                {
                    float2 splatUV3 = terrainUV * _Splat3_ST.xy + _Splat3_ST.zw;
                    fixed4 mask3 = tex2D(_Mask3, splatUV3);
                    float ao3 = ApplyRemap(mask3.g, _MaskRemapMin3.y, _MaskRemapMax3.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao3, control0.a));
                }
                
                // Layer 4-7
                if (control1.r > 0.001)
                {
                    float2 splatUV4 = terrainUV * _Splat4_ST.xy + _Splat4_ST.zw;
                    fixed4 mask4 = tex2D(_Mask4, splatUV4);
                    float ao4 = ApplyRemap(mask4.g, _MaskRemapMin4.y, _MaskRemapMax4.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao4, control1.r));
                }
                
                if (control1.g > 0.001)
                {
                    float2 splatUV5 = terrainUV * _Splat5_ST.xy + _Splat5_ST.zw;
                    fixed4 mask5 = tex2D(_Mask5, splatUV5);
                    float ao5 = ApplyRemap(mask5.g, _MaskRemapMin5.y, _MaskRemapMax5.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao5, control1.g));
                }
                
                if (control1.b > 0.001)
                {
                    float2 splatUV6 = terrainUV * _Splat6_ST.xy + _Splat6_ST.zw;
                    fixed4 mask6 = tex2D(_Mask6, splatUV6);
                    float ao6 = ApplyRemap(mask6.g, _MaskRemapMin6.y, _MaskRemapMax6.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao6, control1.b));
                }
                
                if (control1.a > 0.001)
                {
                    float2 splatUV7 = terrainUV * _Splat7_ST.xy + _Splat7_ST.zw;
                    fixed4 mask7 = tex2D(_Mask7, splatUV7);
                    float ao7 = ApplyRemap(mask7.g, _MaskRemapMin7.y, _MaskRemapMax7.y);
                    mixedOcclusion = min(mixedOcclusion, lerp(1.0, ao7, control1.a));
                }
                
                return fixed4(mixedOcclusion, mixedOcclusion, mixedOcclusion, 1.0);
            }
            ENDCG
        }
    }
}