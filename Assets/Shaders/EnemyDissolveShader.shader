Shader "Custom/EnemyDissolveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolveNoiseTex ("Dissolve Noise Texture", 2D) = "white" {}
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1,0.95,0.7,1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.01, 0.25)) = 0.08
        _DissolveSoftness ("Dissolve Softness", Range(0.001, 0.2)) = 0.04
        _MagicIntensity ("Magic Intensity", Range(0, 5)) = 1.8
        _BaseBrightness ("Base Brightness", Range(0, 1.5)) = 1
        _FinalFadeStart ("Final Fade Start", Range(0, 1)) = 0.75
        _UseProceduralNoise ("Use Procedural Noise", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _DissolveAmount;
            sampler2D _DissolveNoiseTex;
            fixed4 _DissolveEdgeColor;
            float _DissolveEdgeWidth;
            float _DissolveSoftness;
            float _MagicIntensity;
            float _BaseBrightness;
            float _FinalFadeStart;
            float _UseProceduralNoise;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 uv)
            {
                float2 p = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(p);
                float b = Hash21(p + float2(1.0, 0.0));
                float c = Hash21(p + float2(0.0, 1.0));
                float d = Hash21(p + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * _Color;
                fixed4 noiseCol = tex2D(_DissolveNoiseTex, i.uv);

                float proceduralNoise = ValueNoise(i.uv * 12.0) * 0.7 + ValueNoise(i.uv * 32.0) * 0.3;
                float noise = lerp(noiseCol.r, proceduralNoise, saturate(_UseProceduralNoise));

                float threshold = lerp(-_DissolveSoftness, 1.0 + _DissolveSoftness, _DissolveAmount);
                float survive = smoothstep(threshold - _DissolveSoftness, threshold + _DissolveSoftness, noise);
                float edgeDistance = saturate((noise - threshold) / max(_DissolveEdgeWidth, 0.0001));
                float edge = (1.0 - smoothstep(0.0, 1.0, edgeDistance)) * survive;
                float finalFade = 1.0 - smoothstep(_FinalFadeStart, 1.0, _DissolveAmount);

                float darkenProgress = smoothstep(0.0, 0.25, _DissolveAmount);
                fixed3 baseRgb = baseCol.rgb * lerp(1.0, _BaseBrightness, darkenProgress);
                fixed3 magic = _DissolveEdgeColor.rgb * edge * _MagicIntensity;
                fixed3 rgb = lerp(baseRgb, _DissolveEdgeColor.rgb, edge * 0.55) + magic;
                fixed alpha = baseCol.a * survive * finalFade;

                return fixed4(rgb, alpha);
            }
            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}
