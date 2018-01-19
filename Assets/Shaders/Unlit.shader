Shader "Unlit/Unlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma multi_compile NO_TEXTURE _
			#pragma multi_compile NO_COLOR _
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			#if !NO_TEXTURE
				float2 uv : TEXCOORD0;
			#endif
			};

			struct v2f
			{
				#if !NO_TEXTURE
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				#else
				UNITY_FOG_COORDS(0)
				#endif
				float4 vertex : SV_POSITION;
			};

			#if !NO_TEXTURE
			sampler2D _MainTex;
			float4 _MainTex_ST;
			#endif
			#if !NO_COLOR
			fixed4 _Color;
			#endif
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				#if !NO_TEXTURE
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				#endif
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = 1;
				#if !NO_TEXTURE
				col *= tex2D(_MainTex, i.uv);
				#endif
				#if !NO_COLOR
				col *= _Color;
				#endif
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	CustomEditor "UnlitShaderGUI"
}
