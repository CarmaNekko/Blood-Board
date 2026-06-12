Shader "Custom/EnemyOutline"

{

    Properties

    {

        _OutlineColor("Outline Color", Color) = (1, 0, 1, 1)

        _OutlineWidth("Outline Width", Range(0.01, 0.3)) = 0.05

    }



    SubShader

    {

        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }



        Pass

        {

            Name "OUTLINE"

            Cull Front

            ZWrite Off

            ZTest LEqual



            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #include "UnityCG.cginc"



            float _OutlineWidth;

            fixed4 _OutlineColor;



            struct appdata

            {

                float4 vertex : POSITION;

                float3 normal : NORMAL;

            };



            struct v2f

            {

                float4 pos : SV_POSITION;

            };



            v2f vert(appdata v)

            {

                v2f o;



                 float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

                 float4 viewPos = mul(UNITY_MATRIX_MV, v.vertex);

                 viewPos.xyz += viewNormal * _OutlineWidth;

                 o.pos = mul(UNITY_MATRIX_P, viewPos);

                return o;

            }



            fixed4 frag(v2f i) : SV_Target

            {

                return _OutlineColor;

            }

            ENDCG

        }



        Pass

        {

            Name "BASE"

            Cull Back

            ZWrite On

            ZTest LEqual



            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #include "UnityCG.cginc"



            struct appdata

            {

                float4 vertex : POSITION;

            };



            struct v2f

            {

                float4 pos : SV_POSITION;

            };



            v2f vert(appdata v)

            {

                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                return o;

            }



            fixed4 frag(v2f i) : SV_Target

            {

                return fixed4(0, 0, 0, 1);

            }

            ENDCG

        }

    }



    FallBack "Diffuse"

}