Shader "Understand/ShapeShader"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0.2, 0.2, 0.2, 1.0)
		_OutlineColor ("Outline Color", Color) = (0.4, 0.4, 0.4, 1.0)
	}
	SubShader {
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
		LOD 100

		Pass {
			Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
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
			float4 _Color;
			float4 _OutlineColor;
			float4 _MainTex_ST;
			
			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag(v2f i): SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 res;
				res.rgb = max(col.r * _Color.rgb, col.g * _OutlineColor.rgb);
				res.a = col.a;
				return res;
			}
			ENDCG
		}
	}
}
