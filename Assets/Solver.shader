Shader "Hidden/Solver" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BoundaryTex ("Boundary", 2D) = "white" {}
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#define DX 1.0
			#define DIFF (1.0 / (2.0 * DX))
			#define DDIFF (1.0 / (DX * DX))
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

			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			// (u, v, w, rho)
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _BoundaryTex;
			float _Dt;
			float _KVis;
			float _S;
			sampler2D _ForceTex;
			float _ForcePower;

			fixed4 frag (v2f i) : SV_Target {
				float2 duv = _MainTex_TexelSize.xy;
				float4 u = tex2D(_MainTex, i.uv);
				float4 ul = tex2D(_MainTex, i.uv - float2(duv.x, 0));
				float4 ur = tex2D(_MainTex, i.uv + float2(duv.x, 0));
				float4 ub = tex2D(_MainTex, i.uv - float2(0, duv.y));
				float4 ut = tex2D(_MainTex, i.uv + float2(0, duv.y));

				float4 dudx = DIFF * (ur - ul);
				float4 dudy = DIFF * (ut - ub);

				// Mass Conservation (Density)
				float2 rGrad = float2(dudx.w, dudy.w);
				float uDiv = dudx.x + dudy.y;
				u.w -= _Dt * dot(u.xyw, float3(rGrad, uDiv));
				u.w = clamp(u.w, 0.5, 3.0);

				// Momentum Conservation (Velocity)
				u.xy = tex2D(_MainTex, i.uv - _Dt * duv * u.xy).xy;
				float2 f = _ForcePower * (tex2D(_ForceTex, i.uv).xy - 0.5);
				float2 uLaplacian = DDIFF * (ul.xy + ur.xy + ub.xy + ut.xy - 4.0 * u.xy);
				u.xy += _Dt * (-_S * rGrad + f + _KVis * uLaplacian);

				float2 boundary = tex2D(_BoundaryTex, i.uv);
				u.xy *= boundary;

				return u;
			}
			ENDCG
		}
	}
}
