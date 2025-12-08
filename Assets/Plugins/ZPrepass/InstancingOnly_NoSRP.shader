Shader "Testing/InstancingOnly_NoSRP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // ✅ 关键 1：开启 Instancing 变体编译
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ❌ 关键 2：故意不写 CBUFFER_START(UnityPerMaterial)
            // SRP Batcher 依赖该 CBUFFER 来持久化材质数据。
            // 缺少这个，Shader 面板会显示 "SRP Batcher: Not Compatible"

            // ✅ 关键 3：定义 Instancing Buffer
            // 这会让属性支持 per-instance 数据，但因为不在 UnityPerMaterial 里，SRP Batcher 无法接管
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct Attributes
            {
                float4 positionOS : POSITION;
                // ✅ 关键 4：输入结构体必须包含 Instance ID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                // ✅ 关键 5：输出结构体传递 Instance ID 给片元（如果片元需要读取属性）
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // ✅ 关键 6：设置 Instance ID
                UNITY_SETUP_INSTANCE_ID(input);
                // 传递 ID 给片元着色器
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // ✅ 关键 7：在片元着色器重新设置 ID
                UNITY_SETUP_INSTANCE_ID(input);

                // ✅ 关键 8：访问 Instanced 属性
                // 如果开启了 Instancing，它会去查数组；否则读取默认值
                float4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                return color;
            }
            ENDHLSL
        }
    }
}