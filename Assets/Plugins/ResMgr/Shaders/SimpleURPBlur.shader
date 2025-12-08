Shader "Custom/SimpleBlurURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _BlurSize;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 ofs = float2(_BlurSize / 1000.0, _BlurSize / 1000.0);

                float4 col = float4(0,0,0,0);
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-ofs.x, -ofs.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -ofs.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(ofs.x, -ofs.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-ofs.x, 0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(ofs.x, 0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-ofs.x, ofs.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, ofs.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(ofs.x, ofs.y));

                col /= 9.0;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
