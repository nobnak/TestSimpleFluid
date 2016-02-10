Shader "SimpleFluid/FluidEffect" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _ImageTex ("Image", 2D) = "black" {}
        _Weight ("Weight", Vector) = (1, 1, 1, 1)
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
        ColorMask RGB

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _ImageTex;
            float4 _Weight;

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
				float4 csrc = tex2D(_MainTex, i.uv);
                float4 cimage = tex2D(_ImageTex, i.uv);
				return csrc * _Weight.x + cimage * _Weight.y;
			}
			ENDCG
		}
	}
}
