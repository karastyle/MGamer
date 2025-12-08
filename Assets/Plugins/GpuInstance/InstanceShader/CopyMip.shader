Shader "Hidden/CopyMip"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MipLevel("Mip Level", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float _MipLevel;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // 🔧 使用 tex2Dlod 采样指定 mip 层
                float depth = tex2Dlod(_MainTex, float4(i.uv, 0, _MipLevel)).r;
                return float4(depth, depth, depth, 1);
            }
            ENDHLSL
        }
    }
}
