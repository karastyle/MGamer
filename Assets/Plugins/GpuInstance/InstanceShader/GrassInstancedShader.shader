// GrassInstancedShader.shader - 添加接收阴影功能
Shader "Custom/GrassInstancedShader"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 1.0
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0, 1)) = 0.5
        _GrassShadowDistance ("Grass Shadow Distance", Float) = 50.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        // ========================================================================
        // Pass 1: ForwardLit (主渲染) - ✅ 添加接收阴影
        // ========================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            
            // ✅ 添加阴影关键字
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct InstanceData
            {
                float4x4 mat;
                float4 boundsCenter;
                float4 boundsExtents;
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                // ✅ 添加世界坐标和阴影坐标
                float3 positionWS : TEXCOORD4;
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    float4 shadowCoord : TEXCOORD5;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _NormalMap_ST;
                float _NormalScale;
                float _AlphaClipThreshold;
            CBUFFER_END

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<InstanceData> _TransformBuffer;
                StructuredBuffer<uint> _VisibleInstancesBuffer;
                uint _UseCulling;
            #endif

            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    uint instanceIndex = unity_InstanceID;
                    if (_UseCulling > 0)
                    {
                        instanceIndex = _VisibleInstancesBuffer[unity_InstanceID];
                    }
                    
                    unity_ObjectToWorld = _TransformBuffer[instanceIndex].mat;
                    unity_WorldToObject = unity_ObjectToWorld;
                    unity_WorldToObject._14_24_34 = 0;
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // ✅ 计算世界坐标
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
                float sign = input.tangentOS.w * GetOddNegativeScale();
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * sign;

                // ✅ 计算阴影坐标
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = output.positionWS;
                    vertexInput.positionCS = output.positionCS;
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                clip(baseMap.a - _AlphaClipThreshold);

                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalScale);
                float3 normalWS = normalize(
                    normalTS.x * input.tangentWS + normalTS.y * input.bitangentWS + normalTS.z * input.normalWS);

                // ✅ 获取带阴影的主光源
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    Light mainLight = GetMainLight(input.shadowCoord);
                #else
                    Light mainLight = GetMainLight();
                #endif
                
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                // ✅ shadowAttenuation已经包含在mainLight.shadowAttenuation中
                half3 lighting = mainLight.color * mainLight.shadowAttenuation * (NdotL * 0.5 + 0.5);
                half3 finalColor = albedo * lighting;

                return half4(finalColor, baseMap.a);
            }
            ENDHLSL
        }

        // ========================================================================
        // Pass 2: DepthOnly (深度预Pass / HZB 生成源)
        // ========================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct InstanceData
            {
                float4x4 mat;
                float4 boundsCenter;
                float4 boundsExtents;
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _AlphaClipThreshold;
            CBUFFER_END

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<InstanceData> _TransformBuffer;
                StructuredBuffer<uint> _VisibleInstancesBuffer;
                uint _UseCulling;
            #endif

            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    uint instanceIndex = unity_InstanceID;
                    if (_UseCulling > 0)
                    {
                        instanceIndex = _VisibleInstancesBuffer[unity_InstanceID];
                    }
                    
                    unity_ObjectToWorld = _TransformBuffer[instanceIndex].mat;
                    unity_WorldToObject = unity_ObjectToWorld;
                    unity_WorldToObject._14_24_34 = 0;
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
                clip(alpha - _AlphaClipThreshold);
                return 0;
            }
            ENDHLSL
        }

        // ========================================================================
        // Pass 3: ShadowCaster (阴影投射)
        // ========================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct InstanceData
            {
                float4x4 mat;
                float4 boundsCenter;
                float4 boundsExtents;
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _GrassShadowDistance;
                float _AlphaClipThreshold;
            CBUFFER_END

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<InstanceData> _TransformBuffer;
                StructuredBuffer<uint> _VisibleInstancesBuffer;
                uint _UseCulling;
            #endif

            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    uint instanceIndex = unity_InstanceID;
                    if (_UseCulling > 0) instanceIndex = _VisibleInstancesBuffer[unity_InstanceID];
                    unity_ObjectToWorld = _TransformBuffer[instanceIndex].mat;
                    unity_WorldToObject = unity_ObjectToWorld;
                    unity_WorldToObject._14_24_34 = 0;
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                if (_GrassShadowDistance > 0)
                {
                    float dist = distance(positionWS, _WorldSpaceCameraPos);
                    if (dist > _GrassShadowDistance)
                    {
                        output.positionCS = 0;
                        output.uv = 0;
                        return output;
                    }
                }

                float4 positionCS = TransformWorldToHClip(positionWS);

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
                clip(alpha - _AlphaClipThreshold);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
