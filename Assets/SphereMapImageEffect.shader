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
				float fov = 3.14159 * _Fov / 360;

				// scale from [0,1] to [-1,1]
				i.uv = (i.uv * 2) - 1;

				float phi = fov * i.uv.x;
				float theta = fov * i.uv.y;

				float tanTheta = tan(theta);
				float tanPhi = tan(phi);

				float sqTanTheta = tanTheta * tanTheta;
				float sqTanPhi = tanPhi * tanPhi;

				float sqTanFovMul2 = 2 * tan(fov) * tan(fov);
				
				i.uv.x = tan(phi) / tan(fov);

				float h = 1 / sqrt(1 + sqTanFovMul2);
				float a = h / cos(phi);
				float sinDiag = sqrt(1 - (1 / (1 + sqTanFovMul2)));
				i.uv.y = a * tanTheta / sinDiag;
				
				 
				// scale back to [0,1] from [-1,1]
				i.uv = (i.uv * 0.5) + 0.5;

				float4 c = tex2D(_MainTex, i.uv);
				return c;
			}
			ENDCG
		}
	}
}
