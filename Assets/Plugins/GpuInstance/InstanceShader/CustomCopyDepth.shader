Shader "Hidden/CustomCopyDepth"
{
    Properties
    {
        _BlitTexture ("Source Depth", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "CopyDepth"
            ZTest Always 
            ZWrite Off 
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // -----------------------------------------------------------
            // ✅ 关键修复 1：这里什么都不要写！
            // 不要定义 TEXTURE2D_X，也不要定义 SAMPLER
            // Blit.hlsl 全都帮你做好了
            // -----------------------------------------------------------
            
            // ✅ 关键修复 2：显式定义一个 Point Clamp 采样器
            // 深度复制必须精确，不能有插值
            SamplerState my_PointClampSampler;

            float Frag(Varyings input) : SV_Target
            {
                // ✅ 关键修复 3：使用 Point 采样
                // 这里的 _BlitTexture 来自 Blit.hlsl
                float depth = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, my_PointClampSampler, input.texcoord, 0).r;
                
                // ------------------------------------------------
                // 🐛 调试代码：如果你觉得黑，就把下面这行取消注释
                // Reversed-Z 下，深度值很小(0.x)，乘 50 倍变白以便观察
                // return depth * 50.0; 
                // ------------------------------------------------

                return depth;
            }
            ENDHLSL
        }
    }
}