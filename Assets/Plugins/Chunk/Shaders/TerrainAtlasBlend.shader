Shader "Universal Render Pipeline/Terrain/AtlasBlend"
{
    Properties
    {
        _AlbedoAtlas ("Albedo Atlas", 2D) = "white" {}
        _NormalAtlas ("Normal Atlas", 2D) = "bump" {}
        _IndexMap ("Index Map", 2D) = "black" {}
        _BlendMap ("Blend Map", 2D) = "white" {}
        _AtlasUVScaleOffset ("Atlas UV Scale Offset", Vector) = (1,1,0,0)
        _AtlasPadding ("Atlas Padding", Float) = 2.0
        _TileSize ("Tile Size", Float) = 512.0
        _AtlasResolution ("Atlas Resolution", Float) = 2048.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_AlbedoAtlas);
            SAMPLER(sampler_AlbedoAtlas);
            TEXTURE2D(_NormalAtlas);
            SAMPLER(sampler_NormalAtlas);
            TEXTURE2D(_IndexMap);
            SAMPLER(sampler_IndexMap);
            TEXTURE2D(_BlendMap);
            SAMPLER(sampler_BlendMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _AtlasUVScaleOffset;
                float _AtlasPadding;
                float _TileSize;
                float _AtlasResolution;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvAtlas : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };

            // 从图集中采样（考虑padding）
            float4 SampleAtlas(TEXTURE2D_PARAM(atlas, samplerAtlas), float index, float2 uv)
            {
                // 计算tile布局
                float padding = _AtlasPadding;
                float tileSizeWithPadding = _TileSize + padding * 2.0;
                float tilesPerRow = _AtlasResolution / tileSizeWithPadding;
                
                float column = fmod(index, tilesPerRow);
                float row = floor(index / tilesPerRow);
                
                // tile在图集中的起始位置（归一化）
                float2 tileStart = float2(column, row) * tileSizeWithPadding / _AtlasResolution;
                
                // 跳过padding，映射到内容区域
                float2 contentOffset = padding / _AtlasResolution;
                float2 contentScale = _TileSize / _AtlasResolution;
                
                // 最终UV：tileStart + padding偏移 + 内容区域内的uv
                float2 atlasUV = tileStart + contentOffset + frac(uv) * contentScale;
                
                return SAMPLE_TEXTURE2D(atlas, samplerAtlas, atlasUV);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv; // 原始UV，用于Index和Blend
                output.uvAtlas = input.uv * _AtlasUVScaleOffset.xy + _AtlasUVScaleOffset.zw; // 缩放偏移UV，用于图集
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 读取Index和Blend（使用原始UV）
                float3 indices = SAMPLE_TEXTURE2D(_IndexMap, sampler_IndexMap, input.uv).rgb * 15.0;
                float3 blends = SAMPLE_TEXTURE2D(_BlendMap, sampler_BlendMap, input.uv).rgb;

                // 采样三层Albedo（使用uvAtlas）
                float4 albedo1 = SampleAtlas(TEXTURE2D_ARGS(_AlbedoAtlas, sampler_AlbedoAtlas), indices.r, input.uvAtlas);
                float4 albedo2 = SampleAtlas(TEXTURE2D_ARGS(_AlbedoAtlas, sampler_AlbedoAtlas), indices.g, input.uvAtlas);
                float4 albedo3 = SampleAtlas(TEXTURE2D_ARGS(_AlbedoAtlas, sampler_AlbedoAtlas), indices.b, input.uvAtlas);
                float4 albedo = albedo1 * blends.r + albedo2 * blends.g + albedo3 * blends.b;

                // 采样三层Normal（使用uvAtlas）
                float4 normal1 = SampleAtlas(TEXTURE2D_ARGS(_NormalAtlas, sampler_NormalAtlas), indices.r, input.uvAtlas);
                float4 normal2 = SampleAtlas(TEXTURE2D_ARGS(_NormalAtlas, sampler_NormalAtlas), indices.g, input.uvAtlas);
                float4 normal3 = SampleAtlas(TEXTURE2D_ARGS(_NormalAtlas, sampler_NormalAtlas), indices.b, input.uvAtlas);

                // 混合Normal
                float3 tangentNormal1 = UnpackNormal(normal1);
                float3 tangentNormal2 = UnpackNormal(normal2);
                float3 tangentNormal3 = UnpackNormal(normal3);
                float3 tangentNormal = normalize(tangentNormal1 * blends.r + tangentNormal2 * blends.g + tangentNormal3 * blends.b);

                // 转换到世界空间
                float3 normalWS = normalize(
                    tangentNormal.x * input.tangentWS +
                    tangentNormal.y * input.bitangentWS +
                    tangentNormal.z * input.normalWS
                );

                // URP光照
                Light mainLight = GetMainLight();
                half3 lighting = mainLight.color * mainLight.distanceAttenuation * saturate(dot(normalWS, mainLight.direction));
                half3 ambient = SampleSH(normalWS);

                half3 color = albedo.rgb * (lighting + ambient);

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Meta Pass
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            TEXTURE2D(_AlbedoAtlas);
            SAMPLER(sampler_AlbedoAtlas);
            TEXTURE2D(_IndexMap);
            SAMPLER(sampler_IndexMap);
            TEXTURE2D(_BlendMap);
            SAMPLER(sampler_BlendMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _AtlasUVScaleOffset;
                float _AtlasPadding;
                float _TileSize;
                float _AtlasResolution;
            CBUFFER_END

            float4 SampleAtlas(TEXTURE2D_PARAM(atlas, samplerAtlas), float index, float2 uv)
            {
                float padding = _AtlasPadding;
                float tileSizeWithPadding = _TileSize + padding * 2.0;
                float tilesPerRow = _AtlasResolution / tileSizeWithPadding;
                
                float column = fmod(index, tilesPerRow);
                float row = floor(index / tilesPerRow);
                
                float2 tileStart = float2(column, row) * tileSizeWithPadding / _AtlasResolution;
                float2 contentOffset = padding / _AtlasResolution;
                float2 contentScale = _TileSize / _AtlasResolution;
                
                float2 atlasUV = tileStart + contentOffset + frac(uv) * contentScale;
                
                return SAMPLE_TEXTURE2D(atlas, samplerAtlas, atlasUV);
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvAtlas : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST);
                output.uv = input.uv;
                output.uvAtlas = input.uv * _AtlasUVScaleOffset.xy + _AtlasUVScaleOffset.zw;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 indices = SAMPLE_TEXTURE2D(_IndexMap, sampler_IndexMap, input.uv).rgb * 15.0;
                float3 blends = SAMPLE_TEXTURE2D(_BlendMap, sampler_BlendMap, input.uv).rgb;

                float4 albedo1 = SampleAtlas(TEXTURE2D_ARGS(_AlbedoAtlas, sampler_AlbedoAtlas), indices.r, input.uvAtlas);
                float4 albedo2 = SampleAtlas(TEXTURE2D_ARGS(_AlbedoAtlas, sampler_AlbedoAtlas), indices.g, input.uvAtlas);
                float4 albedo3 = SampleAtlas(TEXTURE2D_ARGS(_AlbedoAtlas, sampler_AlbedoAtlas), indices.b, input.uvAtlas);
                float4 albedo = albedo1 * blends.r + albedo2 * blends.g + albedo3 * blends.b;

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = albedo.rgb;
                metaInput.Emission = 0;

                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}