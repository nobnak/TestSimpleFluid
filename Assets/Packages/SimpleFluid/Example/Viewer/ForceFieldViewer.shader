Shader "SimpleFluid/Viewer/ForceFIeld" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _Power ("Power", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		ColorMask RGB

		Pass {
			CGPROGRAM
            #define PI 3.141592
            #define NORMALIZE_RAD (1.0 / (2.0 * PI))

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
            #include "../ColorCollect.cginc"

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
            float _Power;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				float4 c = tex2D(_MainTex, i.uv);
                float h = frac(atan2(c.y, c.x) * NORMALIZE_RAD);
                return float4(hsv2rgb(float3(h, 1, saturate(length(c.xy) * _Power))), 1);
			}
			ENDCG
		}
	}
}
