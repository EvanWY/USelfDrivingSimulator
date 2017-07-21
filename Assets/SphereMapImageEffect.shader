Shader "Custom/SphereMapImageEffect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Fov ("FOV", Float) = 0.333
		//_WidthScale ("Width / Height", Float) = 1
	}
	SubShader {
		Pass {
	  		ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_sphere_mapping
			#include "UnityCG.cginc"
		
			uniform sampler2D _MainTex;

			sampler2D _CameraDepthNormalsTexture;
			sampler2D_float _CameraDepthTexture;

			uniform float _Fov;
			//uniform float _WidthScale;
 
			fixed4 frag_sphere_mapping (v2f_img i) : COLOR {	
				float fov = 3.14159 * _Fov / 360;

				// scale from [0,1] to [-1,1]
				i.uv = (i.uv * 2) - 1;

				float phi = fov * i.uv.x;
				float theta = fov * i.uv.y;

				float tanFov = tan(fov); 

				i.uv.x = tan(phi) / tanFov;
				i.uv.y = tan(theta) / (cos(phi) * tanFov);
				 
				// scale back to [0,1] from [-1,1]
				i.uv = (i.uv * 0.5) + 0.5;

				//float4 c = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				float4 c = tex2D(_MainTex, i.uv);

				//return 2 / (1+ exp(-10 * c)) - 1;

				return c;
			}
			ENDCG
		}
	}
}
