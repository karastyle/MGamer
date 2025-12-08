Shader "Custom/TestShaderFeature_Complex"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _WaveHeight ("Wave Height", Range(0, 1)) = 0.1
        
        // 为了方便在编辑器里测试，通常需要添加 [Toggle] 或 [KeywordEnum] 
        // 这样材质面板上勾选时，Unity会自动帮你 EnableKeyword
        [KeywordEnum(None, Red, Blue)] _ColorMode ("Color Mode", Float) = 0
        [Toggle(ENABLE_WAVE)] _EnableWave ("Enable Wave", Float) = 0
        [Toggle(ENABLE_INVERT)] _EnableInvert ("Enable Invert", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // ---------------------------------------------------------
            // 改为 shader_feature
            // 只有被材质引用了，或者在 ShaderVariantCollection 里记录了，
            // 这些变体才会打进 AB 包。
            // ---------------------------------------------------------
            
            // 对应属性 _ColorMode
            #pragma shader_feature _ _COLORMODE_RED _COLORMODE_BLUE
            
            // 对应属性 _EnableWave
            #pragma shader_feature _ ENABLE_WAVE
            
            // 对应属性 _EnableInvert
            #pragma shader_feature _ ENABLE_INVERT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            float _WaveHeight;

            Varyings vert(Attributes input)
            {
                Varyings output;

                #if defined(ENABLE_WAVE)
                    float wave = sin(_Time.y * 5.0 + input.positionOS.x * 5.0) * _WaveHeight;
                    input.positionOS.y += wave;
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;

                // 注意：这里宏的名字变了，因为用了 [KeywordEnum]
                #if defined(_COLORMODE_RED)
                    texColor.rgb *= half3(1, 0.3, 0.3);
                #elif defined(_COLORMODE_BLUE)
                    texColor.rgb *= half3(0.3, 0.3, 1);
                #endif

                #if defined(ENABLE_INVERT)
                    texColor.rgb = 1.0 - texColor.rgb;
                #endif

                return texColor;
            }
            ENDHLSL
        }
    }
}