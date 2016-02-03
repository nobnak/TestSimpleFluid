Shader "Unlit/Viewer" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Scale ("Scale", Float) = 1
		_DensityMode ("Density Mode", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
			#define PI 3.14159265359
			#define RAD2NORM (1.0 / (2.0 * PI))
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Scale;
			float _DensityMode;

			float3 HUE2RGB(float h) {
				float r = abs(h * 6 - 3) - 1;
				float g = 2 - abs(h * 6 - 2);
				float b = 2 - abs(h * 6 - 4);
				return saturate(float3(r, g, b));
			}
			float3 HSV2RGB(float3 hsv) {
				float3 rgb = HUE2RGB(hsv.x);
				return ((rgb - 1) * hsv.y + 1) * hsv.z;
			}
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				float4 u = _Scale * tex2D(_MainTex, i.uv);
				float h = frac(atan2(u.y, u.x) * RAD2NORM);
				float4 c = float4(HSV2RGB(float3(h, 1, saturate(length(u.xy)))), 1);

				return lerp(c, u.w, _DensityMode);
			}
			ENDCG
		}
	}
}
