Shader "Custom/EnemyOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1, 0, 1, 1)
        _OutlineWidth("Outline Width", Range(0.01, 0.3)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry+1"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "LightMode" = "UniversalForwardOnly"
        }

        Pass
        {
            Name "OUTLINE"
            
            Cull Front
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float _OutlineWidth;
            float4 _OutlineColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
                float3 viewNormal = worldNormal;
                float3 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0)).xyz;
                viewPos.xyz += viewNormal * _OutlineWidth;
                output.positionCS = mul(UNITY_MATRIX_P, float4(viewPos, 1.0));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Forward"
}