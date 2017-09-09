Shader "Custom/SphereMapImageEffect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Fov ("FOV", Float) = 0.333
		_SupersampleScale ("actual sample size", Int) = 2
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
			uniform int _SupersampleScale;
			//uniform float _WidthScale;
 
			fixed4 frag_sphere_mapping (v2f_img i) : COLOR {	
				float fov = 3.14159 * _Fov / 360;

				// scale from [0,1] to [-1,1]
				//i.uv = (i.uv * 2) - 1;
				i.uv = (i.uv * _SupersampleScale * 2) - _SupersampleScale;

				float phi = fov * i.uv.x;
				float theta = fov * i.uv.y;

				float tanFov = tan(fov); 

				i.uv.x = tan(phi) / tanFov;
				i.uv.y = tan(theta) / (cos(phi) * tanFov);
				 
				// scale back to [0,1] from [-1,1]
				i.uv = (i.uv * 0.5) + 0.5;

				float linearDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				float linearDistance = linearDepth/abs(cos(phi) * cos(theta));
				float logisticDistance = 2 / (1+ exp(-10 * linearDistance)) - 1;
				return logisticDistance;

				//float4 c = tex2D(_MainTex, i.uv);
				//return c;
			}
 
			fixed4 frag (v2f_img i) : COLOR {	
				// float fov = 3.14159 * _Fov / 360;

				// // scale from [0,1] to [-1,1]
				// i.uv = (i.uv * 2) - 1;

				// float phi = fov * i.uv.x;
				// float theta = fov * i.uv.y;
				// float tanFov = tan(fov); 

				// i.uv.x = tan(phi) / tanFov;
				// i.uv.y = tan(theta) / (cos(phi) * tanFov);
				 
				// // scale back to [0,1] from [-1,1]
				// i.uv = (i.uv * 0.5) + 0.5;
				
				// float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				// float distance = depth / abs( cos(phi) * cos(theta) );
				// return distance;

				// float logisticDepth = 2 / (1+ exp(-15 * linearDepth)) - 1;
				// float logisticDistance = 2 / (1+ exp(-15 * linearDistance)) - 1;
				// return logisticDistance;

				float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				return 2 / (1+ exp(-15 * depth)) - 1;
			}
			ENDCG
		}
	}
}
