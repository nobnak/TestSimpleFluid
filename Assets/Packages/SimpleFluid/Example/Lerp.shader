Shader "SimpleFluid/Lerp" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _Rate ("Rate", Range(0, 1)) = 0.1
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Rate;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target {
				float4 c = tex2D(_MainTex, i.uv);
                return float4(c.rgb, _Rate);
			}
			ENDCG
		}
	}
}
