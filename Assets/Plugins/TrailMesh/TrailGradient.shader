Shader "Custom/TrailGradient"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,0.2,0.2,1)
        _GradientColor ("Gradient Color", Color) = (1,1,1,0)
        _FadePower ("Fade Power", Range(0,5)) = 1
        _MainTex ("Main Texture (optional)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "TrailPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GradientColor;
                float _FadePower;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // 从头到尾的渐变混合
                half4 color = lerp(_BaseColor, _GradientColor, uv.y);

                // 根据纵向渐变做淡出
                float alphaFade = pow(uv.y, _FadePower);
                color.a *= alphaFade;
                
                return color;
            }
            ENDHLSL
        }
    }
}
