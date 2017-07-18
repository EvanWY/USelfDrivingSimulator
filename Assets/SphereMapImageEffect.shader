Shader "Custom/SphereMapImageEffect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Fov ("FOV", Float) = 0.333
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"
		
			uniform sampler2D _MainTex;
			uniform sampler2D _MaskTex;

			uniform float _Fov;
 
			fixed4 frag (v2f_img i) : COLOR {	
				i.uv = (i.uv * 2) - 1;
				float ang = _Fov * 0.5;
				i.uv.x = tan(ang*i.uv.x) / tan(ang);
				i.uv.y = tan(ang*i.uv.y) / tan(ang);
				i.uv = (i.uv * 0.5) + 0.5;

				float4 c = tex2D(_MainTex, i.uv);
				return c;
			}
			ENDCG
		}
	}
}
