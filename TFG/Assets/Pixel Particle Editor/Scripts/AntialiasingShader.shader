Shader "PixelParticle/Antialiasing Shader"
{
	Properties{
		[HideInInspector]_MainTex("Texture", 2D) = "white" {}
	_BlurSize("Blur Size", Range(0,0.1)) = 0
		[KeywordEnum(Low, Medium, High)] _Samples("Sample amount", Float) = 0
	}
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float _BlurSize;

#pragma multi_compile _SAMPLES_LOW _SAMPLES_MEDIUM _SAMPLES_HIGH
#if _SAMPLES_LOW
#define SAMPLES 10
#elif _SAMPLES_MEDIUM
#define SAMPLES 30
#else
#define SAMPLES 100
#endif

			//the fragment shader
			fixed4 frag(v2f i) : SV_TARGET{
				float invAspect = _ScreenParams.y / _ScreenParams.x;
				//init color variable
				float4 col = 0;
				//iterate over blur samples
				for (float index = 0; index < SAMPLES; index++) {
					//get uv coordinate of sample
					float2 uv = i.uv + float2((index / (SAMPLES - 1) - 0.5) * _BlurSize* invAspect, (index / (SAMPLES - 1) - 0.5) * _BlurSize* invAspect);
					//add color at position to color
					col += tex2D(_MainTex, uv);
				}
				//divide the sum of values by the amount of samples
				col = col / SAMPLES;
				return col;
			}
            ENDCG
        }
    }
}
