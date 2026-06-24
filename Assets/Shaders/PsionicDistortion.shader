Shader "BloodBoard/PsionicDistortionURP"
{
    Properties
    {
        _MainTex ("Distortion Mask (A)", 2D) = "white" {}
        [HDR] _Color ("Tint Color", Color) = (0.2, 0.8, 1.0, 1) // <-- Color del material
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.2     // <-- Qué tanto pinta el fondo
        _DistortionStrength ("Distortion Strength", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
        }

        Pass
        {
            Name "PsionicPass"
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Librerías de URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float _DistortionStrength;
            float4 _Color;        // <-- Variable de Color
            float _TintStrength;  // <-- Variable de Intensidad

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                
                // Calculamos la posición en pantalla para muestrear el fondo
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Leemos la máscara de distorsión
                float4 mask = tex2D(_MainTex, input.uv);
                
                // MULTIPLICAMOS: Color de Partícula (Shuriken) * Color del Material
                float4 finalColor = input.color * _Color;
                
                // Calculamos el centro para empujar los UVs hacia afuera (efecto explosión)
                float2 distortionDir = input.uv - 0.5;
                // La distorsión ahora respeta el alpha combinado
                float2 offset = distortionDir * _DistortionStrength * mask.a * finalColor.a;

                // Proyectamos las coordenadas de pantalla y añadimos la distorsión
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortedUV = screenUV + offset;

                // Muestreamos la "Opaque Texture" (lo que hay detrás de la partícula)
                half3 sceneColor = SampleSceneColor(distortedUV);

                // Mezclamos el fondo distorsionado con el COLOR DEL MATERIAL
                // Usamos _TintStrength para decidir qué tan invasivo es el color
                half3 finalRGB = lerp(sceneColor, finalColor.rgb, _TintStrength * mask.a);

                return half4(finalRGB, mask.a * finalColor.a);
            }
            ENDHLSL
        }
    }
}