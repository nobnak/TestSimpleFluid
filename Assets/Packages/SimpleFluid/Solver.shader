Shader "SimpleFluid/Solver" {
	Properties {
		_FluidTex ("Fluid", 2D) = "white" {}
		_ImageTex ("Image", 2D) = "black" {}
		_ForceTex ("Force", 2D) = "black" {}
		_BoundaryTex ("Boundary", 2D) = "white" {}
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			#define DX 1.0
			#define DIFF (1.0 / (2.0 * DX))
			#define DDIFF (1.0 / (DX * DX))
			#pragma target 5.0
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// (u, v, w, rho)
			sampler2D _FluidTex;
			float4 _FluidTex_TexelSize;
			sampler2D _ImageTex;
			float4 _ImageTex_TexelSize;

			sampler2D _BoundaryTex;
			float _Dt;
			float _KVis;
			float _S;
			sampler2D _ForceTex;
			float4 _ForceTex_ST;
			float _ForcePower;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
		ENDCG

		// Fluid
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target {
				float2 duv = _FluidTex_TexelSize.xy;
				float4 u = tex2D(_FluidTex, i.uv);
				float4 ul = tex2D(_FluidTex, i.uv - float2(duv.x, 0));
				float4 ur = tex2D(_FluidTex, i.uv + float2(duv.x, 0));
				float4 ub = tex2D(_FluidTex, i.uv - float2(0, duv.y));
				float4 ut = tex2D(_FluidTex, i.uv + float2(0, duv.y));

				float2 uLaplacian = DDIFF * (ul.xy + ur.xy + ub.xy + ut.xy - 4.0 * u.xy);

				float4 dudx = DIFF * (ur - ul);
				float4 dudy = DIFF * (ut - ub);

				// Mass Conservation (Density)
				float2 rGrad = float2(dudx.w, dudy.w);
				float uDiv = dudx.x + dudy.y;
				u.w -= _Dt * dot(u.xyw, float3(rGrad, uDiv));
				u.w = clamp(u.w, 0.3, 1.7);

				// Momentum Conservation (Velocity)
				u.xy = tex2D(_FluidTex, i.uv - _Dt * duv * u.xy).xy;
				float4 fTex = tex2D(_ForceTex, i.uv);
				float2 f = _ForcePower * fTex.xy;
				u.xy += _Dt * (-_S * rGrad + f + _KVis * uLaplacian);

				float2 boundary = tex2D(_BoundaryTex, i.uv);
				u.xy *= boundary;

				return u;
			}
			ENDCG
		}
	}
}
