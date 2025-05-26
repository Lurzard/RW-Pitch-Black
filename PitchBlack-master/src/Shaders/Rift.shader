Shader "Futile/Rift"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("Noise Texture", 2D) = "white" {}
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		//Alphatest Greater 0
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Color(0,0,0,0) }
		Lighting Off
		Cull Off //we can turn backface culling off because we know nothing will be facing backwards

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // material properties
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Noise;
            float _Resolution;

            #define iResolution float3(_Resolution, _Resolution, _Resolution)

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }



            fixed4 frag (v2f i) : SV_Target
            {
                float2 fragCoord = i.uv * _Resolution;
                float2 uvOrig = i.uv;
                float2 uvCentered = (fragCoord*2.-iResolution.xy)/iResolution.y;

                fixed4 col = float4(1, 1, 1, 1);//tex2D(_MainTex, i.uv);

                //col.a *= tex2D(_MainTex, i.uv);

                i.uv += tex2D(_MainTex, i.uv);

                col.a *= tex2D(_MainTex, i.uv).x;

                //col.rgba *= smoothstep(1.0, 0.0, length(uvCentered));
                col.a += tex2D(_MainTex, uvOrig).x;

                col.rgb *= tex2D(_Noise, frac(uvOrig + (_Time / 10.))).x + 0.2;// + tex2D(_MainTex, uvOrig).x;

                col.rgb -= tex2D(_MainTex, uvOrig).x;

                col.rgb *= float3(0.6, 0.2, 1) * 3.;

                return col;
            }
            ENDCG
        }
    }
}
